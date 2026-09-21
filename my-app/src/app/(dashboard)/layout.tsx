'use client'
import React from "react";
import { usePathname } from "next/navigation";
import { Container } from "reactstrap";
import Header from "./layouts/header/Header";
import Sidebar from "./layouts/sidebars/vertical/Sidebar";

const FullLayout = ({ children }: { children: React.ReactNode }) => {
  const pathname = usePathname();
  const [open, setOpen] = React.useState(false);
  const [isCollapsed, setIsCollapsed] = React.useState(false);
  const [mounted, setMounted] = React.useState(false);

  React.useEffect(() => {
    setMounted(true);
  }, []);

  const showMobilemenu = () => {
    setOpen(!open);
  };

  const toggleCollapse = () => {
    setIsCollapsed(!isCollapsed);
  };

  if (!mounted) {
    return null;
  }

  // Tất cả các phân hệ giờ đây đều có layout riêng hiện đại:
  // /admin, /don-vi, /quan-ly-giai, /trong-tai, /truong-ban-trong-tai, /thu-ky
  const hasCustomLayout =
    pathname.startsWith('/admin') ||
    pathname.startsWith('/don-vi') ||
    pathname.startsWith('/quan-ly-giai') ||
    pathname.startsWith('/trong-tai') ||
    pathname.startsWith('/truong-ban-trong-tai') ||
    pathname.startsWith('/thu-ky');

  if (hasCustomLayout) {
    return <>{children}</>;
  }

  return (
    <main>
      <div className="pageWrapper d-md-block d-lg-flex">
        {/******** Sidebar **********/}
        <aside
          className={`sidebarArea shadow ${!open ? "" : "showSidebar"} ${isCollapsed ? "collapsed" : ""}`}
          style={{ backgroundColor: "#4e73df" }}
        >
          <Sidebar
            showMobilemenu={() => showMobilemenu()}
            isCollapsed={isCollapsed}
            toggleCollapse={toggleCollapse}
          />
        </aside>
        {/********Content Area**********/}

        <div className="contentArea">
          {/********header**********/}
          <Header
            showMobmenu={() => showMobilemenu()}
            isCollapsed={isCollapsed}
            toggleCollapse={toggleCollapse}
          />

          {/********Middle Content**********/}
          <Container className="p-4 wrapper" fluid>
            <div>{children}</div>
          </Container>
        </div>
      </div>
    </main>
  );
};

export default FullLayout;
