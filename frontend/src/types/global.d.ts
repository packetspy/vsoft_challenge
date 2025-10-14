// Global TypeScript definitions for DOM APIs
export {};

declare global {
  // Browser APIs
  var window: Window & typeof globalThis;
  var document: Document;
  var HTMLElement: {
    prototype: HTMLElement;
    new (): HTMLElement;
  };
  var confirm: (message?: string) => boolean;

  // Event types
  var MouseEvent: {
    prototype: MouseEvent;
    new (type: string, eventInitDict?: MouseEventInit): MouseEvent;
  };
  var KeyboardEvent: {
    prototype: KeyboardEvent;
    new (type: string, eventInitDict?: KeyboardEventInit): KeyboardEvent;
  };
  var Node: {
    prototype: Node;
    new (): Node;
  };

  // ImportMeta for Vite
  interface ImportMetaEnv {
    readonly VITE_API_BASE_URL: string;
  }

  interface ImportMeta {
    readonly env: ImportMetaEnv;
  }
}
