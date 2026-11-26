// Reads retail barcodes (EAN/UPC) from the device camera.
// Uses the browser's built-in BarcodeDetector where available (Chrome on Android, macOS) and falls back to ZXing
// (loaded on demand) elsewhere, e.g. Safari on iOS.

const FORMATS = ['ean_13', 'ean_8', 'upc_a', 'upc_e'];
const ZXING_URL = 'https://cdn.jsdelivr.net/npm/@zxing/browser@0.1.5/+esm';

let session = null;

export async function start(video, dotNetRef) {
    stop();

    if (!window.isSecureContext || !navigator.mediaDevices?.getUserMedia) {
        throw new Error('The camera is only available when the app is opened over HTTPS (or on localhost).');
    }

    let stream;
    try {
        stream = await navigator.mediaDevices.getUserMedia({
            video: { facingMode: { ideal: 'environment' }, width: { ideal: 1280 }, height: { ideal: 720 } },
            audio: false
        });
    } catch (e) {
        throw new Error(e?.name === 'NotAllowedError'
            ? 'Camera access was denied. Allow it in your browser settings, or type the barcode instead.'
            : 'No camera could be opened. Type the barcode instead.');
    }

    const current = { stream, video, active: true, timer: null, controls: null };
    session = current;

    video.srcObject = stream;
    video.muted = true;
    video.setAttribute('playsinline', '');
    await video.play();

    const report = code => {
        if (!current.active) return;
        stop();
        dotNetRef.invokeMethodAsync('OnBarcodeDetected', code);
    };

    if ('BarcodeDetector' in window) {
        const supported = await window.BarcodeDetector.getSupportedFormats();
        const formats = FORMATS.filter(f => supported.includes(f));
        if (formats.length > 0) {
            const detector = new window.BarcodeDetector({ formats });
            const tick = async () => {
                if (!current.active) return;
                try {
                    const codes = await detector.detect(video);
                    if (codes.length > 0) return report(codes[0].rawValue);
                } catch { /* frame not ready yet */ }
                current.timer = setTimeout(tick, 150);
            };
            tick();
            return;
        }
    }

    const { BrowserMultiFormatReader } = await import(ZXING_URL);
    if (!current.active) return;
    current.controls = await new BrowserMultiFormatReader().decodeFromVideoElement(video, result => {
        if (result) report(result.getText());
    });
}

export function stop() {
    const current = session;
    if (!current) return;

    session = null;
    current.active = false;
    clearTimeout(current.timer);
    current.controls?.stop();
    current.stream.getTracks().forEach(track => track.stop());
    current.video.srcObject = null;
}
