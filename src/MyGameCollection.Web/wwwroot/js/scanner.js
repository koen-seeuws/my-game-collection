// Barcode scanning helpers around html5-qrcode (vendored in /lib so the app works without internet access).
let scanner = null;
let libLoading = null;

function loadLib() {
    if (window.__Html5QrcodeLibrary__) return Promise.resolve();
    if (!libLoading) {
        libLoading = new Promise((resolve, reject) => {
            const s = document.createElement('script');
            s.src = '/lib/html5-qrcode/html5-qrcode.min.js';
            s.onload = resolve;
            s.onerror = () => reject(new Error('Could not load the scanner library.'));
            document.head.appendChild(s);
        });
    }
    return libLoading;
}

const lib = () => window.__Html5QrcodeLibrary__ ?? window;

function formats() {
    const f = lib().Html5QrcodeSupportedFormats;
    return [f.EAN_13, f.EAN_8, f.UPC_A, f.UPC_E, f.CODE_128, f.CODE_39];
}

export function isLiveSupported() {
    return window.isSecureContext && !!(navigator.mediaDevices && navigator.mediaDevices.getUserMedia);
}

export async function start(elementId, dotnetRef, cameraId) {
    try {
        await loadLib();
        await stop();
        scanner = new (lib().Html5Qrcode)(elementId, {
            formatsToSupport: formats(),
            useBarCodeDetectorIfSupported: true,
            verbose: false
        });
        const camera = cameraId ? { deviceId: { exact: cameraId } } : { facingMode: 'environment' };
        await scanner.start(
            camera,
            {
                fps: 12,
                // A wide, short box suits 1D barcodes better than the default square.
                qrbox: (w, h) => ({ width: Math.floor(Math.min(w * 0.9, 420)), height: Math.floor(Math.min(h * 0.45, 180)) }),
                aspectRatio: 1.333
            },
            (text) => {
                if (navigator.vibrate) navigator.vibrate(80);
                stop();
                dotnetRef.invokeMethodAsync('OnBarcodeScanned', text);
            },
            () => { /* no barcode in this frame */ });

        let activeId = cameraId;
        try {
            const settings = scanner.getRunningTrackSettings?.();
            activeId = settings?.deviceId ?? cameraId;
        } catch { /* not all browsers expose this */ }
        return { ok: true, error: null, cameraId: activeId ?? null };
    } catch (err) {
        const msg = (err && (err.message || err.name)) || String(err);
        const friendly = /NotAllowed|Permission/i.test(msg)
            ? 'Camera permission was denied. Allow camera access in your browser settings.'
            : /NotFound|Requested device not found/i.test(msg)
                ? 'No camera found on this device.'
                : msg;
        return { ok: false, error: friendly, cameraId: null };
    }
}

export async function listCameras() {
    try {
        const cams = await lib().Html5Qrcode.getCameras();
        return cams.map((c, i) => ({ id: c.id, label: c.label || `Camera ${i + 1}` }));
    } catch {
        return [];
    }
}

export async function stop() {
    if (!scanner) return;
    const s = scanner;
    scanner = null;
    try {
        if (s.isScanning) await s.stop();
        s.clear();
    } catch { /* already stopped */ }
}

// Decodes a barcode from an image streamed from .NET (photo mode; works without HTTPS).
export async function scanImage(streamRef) {
    await loadLib();
    const buffer = await streamRef.arrayBuffer();
    const file = new File([buffer], 'barcode.jpg', { type: 'image/jpeg' });

    let host = document.getElementById('scanner-file-host');
    if (!host) {
        host = document.createElement('div');
        host.id = 'scanner-file-host';
        host.style.display = 'none';
        document.body.appendChild(host);
    }
    const fileScanner = new (lib().Html5Qrcode)(host.id, { formatsToSupport: formats(), useBarCodeDetectorIfSupported: true, verbose: false });
    try {
        return await fileScanner.scanFile(file, false);
    } catch {
        return null;
    } finally {
        try { fileScanner.clear(); } catch { }
    }
}
