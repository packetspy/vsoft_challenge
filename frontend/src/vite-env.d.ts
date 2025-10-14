/// <reference types="vite/client" />

declare module '*.vue' {
  import type { DefineComponent } from 'vue';
  const component: DefineComponent<{}, {}, any>;
  export default component;
}

// CSS modules
declare module '*.css' {
  const content: string;
  export default content;
}

// Define import.meta.env
interface ImportMetaEnv {
  readonly VITE_API_BASE_URL: string;
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}

// Define global types for browser APIs
declare var window: Window & typeof globalThis;
declare var document: Document;
declare var HTMLElement: {
  prototype: HTMLElement;
  new (): HTMLElement;
};
declare var MouseEvent: {
  prototype: MouseEvent;
  new (type: string, eventInitDict?: MouseEventInit): MouseEvent;
};
declare var KeyboardEvent: {
  prototype: KeyboardEvent;
  new (type: string, eventInitDict?: KeyboardEventInit): KeyboardEvent;
};
declare var Node: {
  prototype: Node;
  new (): Node;
};
