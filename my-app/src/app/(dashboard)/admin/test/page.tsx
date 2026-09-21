'use client'
import { Row, Col, Table, Card, CardTitle, CardBody,CardSubtitle } from "reactstrap";
const tableData = [
  {
    // avatar: user1,
    name: "Hanna Gover",
    email: "hgover@gmail.com",
    project: "Flexy React",
    status: "pending",
    weeks: "35",
    budget: "95K",
  },
  {
    // avatar: user2,
    name: "Hanna Gover",
    email: "hgover@gmail.com",
    project: "Lading pro React",
    status: "done",
    weeks: "35",
    budget: "95K",
  },
  
];
const Tables = () => {
    return (
        <Row>
           
            <Col lg="12">
                <Card>
                    <CardBody>
                        <CardTitle tag="h5">Project Listing</CardTitle>
                        <CardSubtitle className="mb-2 text-muted" tag="h6">
                            Overview of the projects
                        </CardSubtitle>
                        <div className="table-responsive">
                            <Table className="text-nowrap mt-3 align-middle" borderless>
                                <thead>
                                    <tr>
                                        <th>Team Lead</th>
                                        <th>Project</th>

                                        <th>Status</th>
                                        <th>Weeks</th>
                                        <th>Budget</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {tableData.map((tdata, index) => (
                                        <tr key={index} className="border-top">
                                            <td>
                                                <div className="d-flex align-items-center p-2">
                                                    {/* <Image
                                                        src={tdata.avatar}
                                                        className="rounded-circle"
                                                        alt="avatar"
                                                        width="45"
                                                        height="45"
                                                    /> */}
                                                    <div className="ms-3">
                                                        <h6 className="mb-0">{tdata.name}</h6>
                                                        <span className="text-muted">{tdata.email}</span>
                                                    </div>
                                                </div>
                                            </td>
                                            <td>{tdata.project}</td>
                                            <td>
                                                {tdata.status === "pending" ? (
                                                    <span className="p-2 bg-danger rounded-circle d-inline-block ms-3" />
                                                ) : tdata.status === "holt" ? (
                                                    <span className="p-2 bg-warning rounded-circle d-inline-block ms-3" />
                                                ) : (
                                                    <span className="p-2 bg-success rounded-circle d-inline-block ms-3" />
                                                )}
                                            </td>
                                            <td>{tdata.weeks}</td>
                                            <td>{tdata.budget}</td>
                                        </tr>
                                    ))}
                                </tbody>
                            </Table>
                        </div>
                    </CardBody>
                </Card>
            </Col>
           
        </Row>
    );
};

export default Tables;
