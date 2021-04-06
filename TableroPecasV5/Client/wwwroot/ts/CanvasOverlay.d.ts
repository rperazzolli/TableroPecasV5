declare module "CanvasOverlayModule.js" {
    interface CanvasOverlayCallback { (name: HTMLCanvasElement): void }
    export function CanvasOverlay(Fnc: CanvasOverlayCallback): void;
}
