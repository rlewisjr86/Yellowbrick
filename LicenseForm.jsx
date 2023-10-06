import React, { useState, useEffect } from "react";
import { Formik, Form, Field, ErrorMessage } from "formik";
import licenseSchema from "schemas/licenseSchema";
import lookUpService from "services/lookUpService";
import debug from "Yellowbrick-debug";
import licenseService from "services/licenseService";
import toastr from "toastr";
import { useLocation } from "react-router-dom";
import { formatDateOnly } from "../../utils/dateFormater.js";

const _logger = debug.extend("LicenseForm");

function LicenseForm() {
  const initialValues = {
    licenseTypeId: "",
    licenseStateId: "",
    licenseNumber: "",
    dateExpiration: "",
    issueDate: "",
    isFederal: false,
  };
  const [license, setLicense] = useState(initialValues);
  const [pageData, setPageData] = useState({
    licenses: [],
    mapLicenses: [],
  });
  const location = useLocation();
  const [lookups, setLookup] = useState({
    licenseTypes: [],
    mappedLicenses: [],
    states: [],
    mappedStates: [],
  });

  _logger(license, pageData, setPageData);

  useEffect(() => {
    lookUpService
      .lookUp(["LicenseType", "States"])
      .then(lookUpSuccess)
      .catch(lookUpError);
    const state = location;

    if (state.state && state.state.payload) {
      const editedLicense = state.state.payload;
      _logger("Edited License:", editedLicense);
      setLicense((prevState) => ({
        ...prevState,
        licenseTypeId: editedLicense.licenseType.id,
        licenseStateId: editedLicense.licenseState.id,
        licenseNumber: editedLicense.licenseNumber,
        dateExpiration: formatDateOnly(editedLicense.dateExpiration),
        issueDate: formatDateOnly(editedLicense.issueDate),
        isFederal: editedLicense.isFederal,
      }));
    }
    _logger(state, "state", location);
  }, [location, setLicense]);

  const lookUpSuccess = (response) => {
    const { licenseType, states } = response.item;
    setLookup((prevState) => {
      const license = { ...prevState };
      license.licenseTypes = licenseType;
      license.mappedLicenses = licenseType.map(mapTwoColumnLookUp);
      license.states = states;
      license.mappedStates = states.map(mapTwoColumnLookUp);
      return license;
    });
  };

  const lookUpError = (e) => {
    _logger(e);
  };

  const onLicenseSubmit = (values, resetForm) => {
    values.dateExpiration = formatDateOnly(values.dateExpiration);
    values.issueDate = formatDateOnly(values.issueDate);
    values.isFederal = values.isFederal ? 1 : 0;
    _logger(values);

    if (location.state) {
      licenseService
        .updateLicense(location.state.payload.id, values)
        .then(onLicenseUpdateSuccess)
        .catch(onLicenseUpdateError);
    } else {
      licenseService
        .create(values)
        .then((response) => onLicenseAddSuccess(response, resetForm))
        .catch(onLicenseAddError);
    }
  };

  const onLicenseAddSuccess = (response, resetForm) => {
    _logger(response);
    toastr.success("License Added Successfully");
    setPageData((prevState) => {
      const newState = { ...prevState };
      newState.licenses = [...newState.licenses, response];
      newState.mapLicenses = newState.licenses.map(mapLicenses);
      _logger(newState);
      return newState;
    });
    resetForm(initialValues);
  };

  const onLicenseAddError = (error) => {
    _logger(error);
  };

  const onLicenseUpdateSuccess = (response, resetForm) => {
    _logger(response);
    toastr.success("License Updated Successfully");
    setLicense((prevState) => {
      const updateLicense = location.state.payload;
      return { ...prevState, ...updateLicense };
    });
    resetForm();
  };

  const onLicenseUpdateError = (error) => {
    _logger(error);
  };

  const mapLicenses = (license, index) => {
    return (
      <div key={index} className="license-item">
        <h4>License #{index + 1}</h4>
        {/* <div>
          <strong>License Type:</strong> {licenseTypeName}
        </div>
        <div>
          <strong>State:</strong> {stateName}
        </div> */}
        <div>
          <strong>License Number:</strong> {license.licenseNumber}
        </div>
        <div>
          <strong>Issue Date:</strong> {license.issueDate}
        </div>
        <div>
          <strong>Expiration Date:</strong> {license.dateExpiration}
        </div>
        <div>
          <strong>Is Federal:</strong> {license.isFederal ? "Yes" : "No"}
        </div>
      </div>
    );
  };

  const mapTwoColumnLookUp = (item) => (
    <option key={item.id} value={item.id}>
      {item.name}
    </option>
  );

  return (
    <React.Fragment>
      <div>
        <nav aria-label="breadcrumb">
          <ol className="breadcrumb">
            <li className="breadcrumb-item">
              <a href="/agent">Agent List</a>
            </li>
            <li className="breadcrumb-item active" aria-current="page">
              Add License
            </li>
          </ol>
        </nav>
      </div>
      <div className="col-xl-6 offset-xl-3 col-md-12 col-xs-12">
        <div className="card">
          <div className="card-body p-lg-6 bg-white">
            <div className="row">
              <Formik
                enableReinitialize={true}
                initialValues={license}
                validationSchema={licenseSchema}
                onSubmit={(values, { resetForm }) => {
                  onLicenseSubmit(values, resetForm);
                }}
              >
                {({ values }) => (
                  <Form>
                    <div className="form-group mb-3">
                      <label htmlFor="licenseTypeId">License Type</label>
                      <Field
                        component="select"
                        name="licenseTypeId"
                        className="form-control"
                        id="formLicenseType"
                      >
                        <option value="0">Please select a license type</option>
                        {lookups.mappedLicenses}
                      </Field>
                      <ErrorMessage
                        name="licenseTypeId"
                        component="div"
                        className="has-error"
                      />
                    </div>
                    <div className="form-group mb-3">
                      <label htmlFor="licenseStateId">State Id</label>
                      <Field
                        component="select"
                        type="text"
                        name="licenseStateId"
                        className="form-control"
                      >
                        <option value="0">Please select a State </option>
                        {lookups.mappedStates}
                      </Field>
                      <ErrorMessage
                        name="licenseStateId"
                        component="div"
                        className="has-error"
                      />
                    </div>
                    <div className="form-group mb-3">
                      <label htmlFor="licenseNumber">License Number</label>
                      <Field
                        component="input"
                        type="text"
                        name="licenseNumber"
                        className="form-control"
                      />
                      <ErrorMessage
                        name="licenseNumber"
                        component="div"
                        className="has-error"
                      />
                    </div>
                    <div className="form-group mb-3">
                      <label htmlFor="issueDate">Issue Date</label>
                      <Field
                        component="input"
                        type="date"
                        name="issueDate"
                        className="form-control"
                        value={values.issueDate}
                      />
                      <ErrorMessage
                        name="issueDate"
                        component="div"
                        className="has-error"
                      />
                    </div>
                    <div className="form-group mb-3">
                      <label htmlFor="dateExpiration">Expiration Date</label>
                      <Field
                        component="input"
                        type="date"
                        name="dateExpiration"
                        className="form-control"
                        value={values.dateExpiration}
                      />
                      <ErrorMessage
                        name="dateExpiration"
                        component="div"
                        className="has-error"
                      />
                    </div>

                    <div className="form-check">
                      <Field
                        type="checkbox"
                        name="isFederal"
                        className="form-check-input"
                      />{" "}
                      Is this a Federal License?
                      <label
                        htmlFor="isFederal"
                        className="form-check-label"
                      ></label>
                      <ErrorMessage
                        name="isFederal"
                        component="div"
                        className="has-error"
                      />
                    </div>
                    <div className="row">
                      <div className="col-2"></div>
                    </div>
                    <button type="submit" className=" btn btn-primary">
                      Submit
                    </button>
                  </Form>
                )}
              </Formik>
              <div className="col-md-6">{pageData.mapLicenses}</div>
            </div>
          </div>
        </div>
      </div>
    </React.Fragment>
  );
}

export default LicenseForm;
