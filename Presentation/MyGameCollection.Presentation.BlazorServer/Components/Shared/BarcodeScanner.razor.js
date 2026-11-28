// Reads EAN / UPC barcodes from the camera. Uses the browser's own BarcodeDetector where it reads those
// (Chrome, Edge, Android); elsewhere (Safari, Firefox) it loads ZXing once.
const ZXING_URL = "https://cdn.jsdelivr.net/npm/@zxing/browser@0.1.5/umd/zxing-browser.min.js";
const FORMATS = ["ean_13", "ean_8", "upc_a", "upc_e"];

const sessions = new WeakMap();
let zxingLoading;

export async function start(video, dotNet) {
    stop(video);

    if (!navigator.mediaDevices?.getUserMedia) {
        throw new Error(window.isSecureContext
            ? "This browser cannot use a camera."
            : "The camera only works over HTTPS. Open the site with https:// to scan.");
    }

    const stream = await navigator.mediaDevices.getUserMedia({
        video: { facingMode: { ideal: "environment" }, width: { ideal: 1280 }, height: { ideal: 720 } },
        audio: false
    });

    const session = { stream, stopped: false, timer: 0, controls: null };
    sessions.set(video, session);

    video.srcObject = stream;
    video.setAttribute("playsinline", "");
    video.muted = true;
    await video.play();

    const found = code => {
        if (session.stopped) return;
        stop(video);
        dotNet.invokeMethodAsync("OnDetected", code);
    };

    if (await nativeDetectorWorks()) {
        const detector = new BarcodeDetector({ formats: FORMATS });
        const scan = async () => {
            if (session.stopped) return;
            try {
                const codes = await detector.detect(video);
                if (codes.length > 0) return found(codes[0].rawValue);
            } catch {
                // The frame was not ready yet: try the next one.
            }
            session.timer = setTimeout(scan, 150);
        };
        scan();
    } else {
        const ZXingBrowser = await loadZxing();
        if (session.stopped) return;
        const reader = new ZXingBrowser.BrowserMultiFormatReader();
        session.controls = await reader.decodeFromVideoElementContinuously(video, result => {
            if (result) found(result.getText());
        });
        if (session.stopped) session.controls?.stop();
    }
}

export function stop(video) {
    const session = video && sessions.get(video);
    if (!session) return;

    session.stopped = true;
    clearTimeout(session.timer);
    session.controls?.stop?.();
    session.stream.getTracks().forEach(track => track.stop());
    video.srcObject = null;
    sessions.delete(video);
}

async function nativeDetectorWorks() {
    if (!("BarcodeDetector" in window)) return false;
    try {
        const supported = await BarcodeDetector.getSupportedFormats();
        return FORMATS.some(format => supported.includes(format));
    } catch {
        return false;
    }
}

function loadZxing() {
    if (window.ZXingBrowser) return Promise.resolve(window.ZXingBrowser);
    zxingLoading ??= new Promise((resolve, reject) => {
        const script = document.createElement("script");
        script.src = ZXING_URL;
        script.crossOrigin = "anonymous";
        script.onload = () => resolve(window.ZXingBrowser);
        script.onerror = () => {
            zxingLoading = undefined;
            reject(new Error("The barcode reader could not be loaded. Check your connection and try again."));
        };
        document.head.appendChild(script);
    });
    return zxingLoading;
}
