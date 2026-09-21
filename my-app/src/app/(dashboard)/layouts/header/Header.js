import React from "react";
import Link from "next/link";
import Image from "next/image";
import {
  Navbar,
  Collapse,
  Nav,
  NavItem,
  NavbarBrand,
  UncontrolledDropdown,
  DropdownToggle,
  DropdownMenu,
  DropdownItem,
  Dropdown,
  Button,
} from "reactstrap";

const Header = ({ showMobmenu, isCollapsed, toggleCollapse }) => {
  const [isOpen, setIsOpen] = React.useState(false);
  const [dropdownOpen, setDropdownOpen] = React.useState(false);

  const toggle = () => setDropdownOpen((prevState) => !prevState);
  const Handletoggle = () => {
    setIsOpen(!isOpen);
  };

  return (
    <Navbar color="primary" dark expand="md">
      <div className="d-flex align-items-center">
        {/* Nút thu gọn/mở rộng sidebar trên desktop */}
        <Button
          color="primary"
          className="d-none d-lg-inline-flex align-items-center justify-content-center me-3 border-0 bg-transparent"
          onClick={toggleCollapse}
          title={isCollapsed ? "Mở rộng menu" : "Thu gọn menu"}
          style={{ width: "38px", height: "38px" }}
        >
          <i className={`bi ${isCollapsed ? 'bi-text-indent-left' : 'bi-text-indent-right'} fs-4 text-white`}></i>
        </Button>

        <NavbarBrand href="/" className="d-lg-none">
          <Image src="/images/logos/logo.png" alt="logo" width={150} height={40} />
        </NavbarBrand>
        <Button color="primary" className="d-lg-none" onClick={showMobmenu}>
          <i className="bi bi-list"></i>
        </Button>
      </div>
      <div className="hstack gap-2">
        <Button
          color="primary"
          size="sm"
          className="d-sm-block d-md-none"
          onClick={Handletoggle}
        >
          {isOpen ? (
            <i className="bi bi-x"></i>
          ) : (
            <i className="bi bi-three-dots-vertical"></i>
          )}
        </Button>
      </div>

      <Collapse navbar isOpen={isOpen}>
        <Nav className="me-auto" navbar>
          <NavItem>
            <Link href="/" className="nav-link">
              Starter
            </Link>
          </NavItem>
          <NavItem>
            <Link href="/pages/about" className="nav-link">
              About
            </Link>
          </NavItem>
          <UncontrolledDropdown inNavbar nav>
            <DropdownToggle caret nav>
              DD Menu
            </DropdownToggle>
            <DropdownMenu end>
              <DropdownItem>Option 1</DropdownItem>
              <DropdownItem>Option 2</DropdownItem>
              <DropdownItem divider />
              <DropdownItem>Reset</DropdownItem>
            </DropdownMenu>
          </UncontrolledDropdown>
        </Nav>
        <Dropdown isOpen={dropdownOpen} toggle={toggle}>
          <DropdownToggle color="primary">
            <div style={{ lineHeight: "0px" }}>
              <Image
                src="/images/users/user1.jpg"
                alt="profile"
                className="rounded-circle"
                width="30"
                height="30"
              />
            </div>
          </DropdownToggle>
          <DropdownMenu>
            <DropdownItem header>Info</DropdownItem>
            <DropdownItem>My Account</DropdownItem>
            <DropdownItem>Edit Profile</DropdownItem>
            <DropdownItem divider />
            <DropdownItem>My Balance</DropdownItem>
            <DropdownItem>Inbox</DropdownItem>
            <DropdownItem>Logout</DropdownItem>
          </DropdownMenu>
        </Dropdown>
      </Collapse>
    </Navbar>
  );
};

export default Header;
