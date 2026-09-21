'use client';

import { useEffect } from 'react';

// Chặn console.error cảnh báo hydration do browser extension (Bitdefender, Grammarly...) tiêm thuộc tính
if (typeof window !== 'undefined') {
  const originalError = console.error;
  console.error = function (...args: any[]) {
    const isExtensionHydrationError = args.some(
      (arg) =>
        typeof arg === 'string' &&
        (arg.includes('bis_skin_checked') ||
          arg.includes('bis_register') ||
          arg.includes('bis_frame_id') ||
          arg.includes('data-gr-ext') ||
          arg.includes('Grammarly'))
    );

    if (isExtensionHydrationError) {
      // Bỏ qua cảnh báo sai lệch do extension trình duyệt tự ý can thiệp DOM
      return;
    }
    originalError.apply(console, args);
  };
}

export default function HydrationFix() {
  useEffect(() => {
    try {
      const dirtyElements = document.querySelectorAll(
        '[bis_skin_checked], [bis_register], [bis_frame_id]'
      );
      dirtyElements.forEach((el) => {
        el.removeAttribute('bis_skin_checked');
        el.removeAttribute('bis_register');
        el.removeAttribute('bis_frame_id');
      });
    } catch {}
  }, []);

  return null;
}
