import type { Metadata } from "next";
import { Geist, Geist_Mono } from "next/font/google";
import "../styles/style.scss";
import "./globals.css";
import { AuthProvider } from "../context/AuthContext";
import HydrationFix from "../components/common/HydrationFix";

const geistSans = Geist({
  variable: "--font-geist-sans",
  subsets: ["latin"],
});

const geistMono = Geist_Mono({
  variable: "--font-geist-mono",
  subsets: ["latin"],
});

export const metadata: Metadata = {
  title: "Hệ Thống Xác Thực & Phân Quyền JWT / RBAC",
  description: "DMS Sports & DienLanh Authentication and RBAC Authorization",
};

export default function RootLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <html
      lang="vi"
      className={`${geistSans.variable} ${geistMono.variable}`}
      suppressHydrationWarning
    >
      <head>
        <script
          dangerouslySetInnerHTML={{
            __html: `
              (function() {
                try {
                  var origSetAttr = Element.prototype.setAttribute;
                  Element.prototype.setAttribute = function(name, val) {
                    if (name === 'bis_skin_checked' || name === 'bis_register' || name === 'bis_frame_id') {
                      return;
                    }
                    return origSetAttr.apply(this, arguments);
                  };
                } catch(e) {}

                try {
                  var observer = new MutationObserver(function(mutations) {
                    for (var i = 0; i < mutations.length; i++) {
                      var m = mutations[i];
                      if (m.type === 'attributes' && m.attributeName && m.attributeName.indexOf('bis_') === 0) {
                        m.target.removeAttribute(m.attributeName);
                      }
                    }
                  });
                  observer.observe(document.documentElement, {
                    attributes: true,
                    subtree: true,
                    attributeFilter: ['bis_skin_checked', 'bis_register', 'bis_frame_id']
                  });
                } catch(e) {}

                try {
                  var origErr = console.error;
                  console.error = function() {
                    for (var i = 0; i < arguments.length; i++) {
                      var a = arguments[i];
                      if (typeof a === 'string' && (a.indexOf('bis_skin_checked') !== -1 || a.indexOf('bis_') !== -1)) {
                        return;
                      }
                    }
                    return origErr.apply(console, arguments);
                  };
                } catch(e) {}
              })();
            `,
          }}
        />
      </head>
      <body suppressHydrationWarning>
        <HydrationFix />
        <AuthProvider>{children}</AuthProvider>
      </body>
    </html>
  );
}
