import React, { useEffect, useState } from "react";
import "rc-pagination/assets/index.css";
import Pagination from "rc-pagination";
import locale from "rc-pagination/lib/locale/en_US";
import debug from "sabio-debug";
import licenseServices from "services/licenseService";
import { Table } from "react-bootstrap";
import LicenseTR from "./LicenseTR";
import { Formik, Form, Field } from "formik";

const _logger = debug.extend("Licenses");

function LicenseList() {
  const [pageData, setPageData] = useState({
    licenses: [],
    mappedLicenses: [],
    pageIndex: 0,
    pageSize: 8,
    totalCount: 0,
    currentPage: 1,
    totalSearchCount: 0,
    searchInput: "",
  });

  const onPageChange = (page) => {
    setPageData((prevState) => {
      let newPage = { ...prevState };
      newPage.currentPage = page;
      _logger(page);
      return newPage;
    });
  };

  useEffect(() => {
    if (pageData.searchInput !== "") {
      licenseServices
        .searchLicenseList(
          pageData.currentPage - 1,
          pageData.pageSize,
          pageData.searchInput
        )
        .then(paginateLicenseSuccess)
        .catch(paginateLicenseError);
    } else {
      licenseServices
        .paginateLicense(pageData.currentPage - 1, pageData.pageSize)
        .then(paginateLicenseSuccess)
        .catch(paginateLicenseError);
    }
  }, [pageData.currentPage, pageData.searchInput]);

  const paginateLicenseError = (e) => {
    _logger(e);
  };

  const paginateLicenseSuccess = (response) => {
    const { pagedItems, totalCount } = response.item;
    _logger(response);
    setPageData((prevState) => {
      const lList = { ...prevState };
      lList.mappedLicenses = pagedItems.map(mapAllRows);
      lList.licenses = pagedItems;
      lList.totalCount = totalCount;
      _logger(totalCount);

      return lList;
    });
  };

  const handleSearch = (e) => {
    const { value } = e.target;

    setPageData((prevState) => {
      let newObj = { ...prevState };
      newObj.searchInput = value;
      newObj.currentPage = 1;
      return newObj;
    });
  };

  const onSearchClick = (value) => {
    if (value.query !== "") {
      setClients((prevState) => {
        const setQuery = { ...prevState };
        setQuery.searchInput = value.query;
        setQuery.currentPage = 1;

        return setQuery;
      });
    }
  };

  const mapAllRows = (aLicense) => {
    return <LicenseTR license={aLicense} key={aLicense.id} />;
  };

  return (
    <div>
      <div className="row">
        <div className="col-lg-12 col-md-12 col-sm-12">
          <div className="border-bottom mb-4 d-md-flex align-items-center justify-content-between">
            <div className="mb-3 mb-md-0">
              <h1 className="mb-1 h2 fw-bold">Licenses </h1>
            </div>
            <Formik
              enableReinitialize={true}
              initialValues={{ query: "" }}
              onSubmit={onSearchClick}
            >
              <Form>
                <div className=" row col- ">
                  <div className="d-flex align-items-center">
                    <Field
                      type="text"
                      name="licenseType"
                      className="form-control me-2"
                      placeholder="Search by License Type"
                      onChange={handleSearch}
                    />
                  </div>
                </div>
              </Form>
            </Formik>
          </div>
        </div>
        <div className="row">
          <div className="col-lg-12 col-md-12 col-sm-12 ">
            <div className="card ">
              <div className="p-0 card-body">
                <div className="table-responsive">
                  <Table
                    className="text-nowrap p-lg-6 bg-white table-hover maintain-cursor"
                    role="table"
                  >
                    <thead className="table-primary ">
                      <tr>
                        <th className="col-4 text-center border border-secondary ">
                          LICENSE TYPE
                        </th>
                        <th className="col-4 text-center border border-secondary ">
                          LOCATION
                        </th>
                        <th className="col-2 text-center border border-secondary ">
                          AGENT
                        </th>
                        <th className="col-1 text-center border border-secondary "></th>
                      </tr>
                    </thead>
                    <tbody role="rowgroup">{pageData.mappedLicenses}</tbody>
                  </Table>
                </div>
                <div className="active-page inactive-page carat-style hover-color pb-3">
                  <Pagination
                    onChange={onPageChange}
                    current={pageData.currentPage}
                    total={pageData.totalCount}
                    pageSize={pageData.pageSize}
                    locale={locale}
                    className="text-center"
                  />
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}

export default LicenseList;
