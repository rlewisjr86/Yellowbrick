import React from "react";
import { useNavigate } from "react-router-dom";
import propTypes from "prop-types";
import debug from "sabio-debug";

const _logger = debug.extend("LicenseComponent");

function LicenseTR({ license }) {
  const navigate = useNavigate();

  const onLicenseClick = () => {
    navigate(`/license/${license.id}/edit`, {
      state: { payload: license, type: "EDIT_TYPE" },
    });
    _logger(navigate);
  };

  return (
    <tr>
      <td className="col-4 text-center border border-secondary bg-white">
        {license.licenseType?.name}
      </td>
      <td className="col-4 text-center border border-secondary bg-white">
        {license.licenseState?.name}
      </td>
      <td className="col-2 text-center border border-secondary bg-white">{`${license.createdBy?.firstName} ${license.createdBy?.lastName}`}</td>
      <td className="col-1 text-center border border-secondary bg-white">
        <i
          className="fe fe-edit fs-3 pe-1 btn btn-link"
          id={license.id}
          onClick={onLicenseClick}
        ></i>
      </td>
    </tr>
  );
}

LicenseTR.propTypes = {
  license: propTypes.shape({
    id: propTypes.number.isRequired,
    licenseType: propTypes.shape({
      id: propTypes.number,
      name: propTypes.string,
    }),
    licenseState: propTypes.shape({
      id: propTypes.number,
      name: propTypes.string,
    }),
    licenseNumber: propTypes.string.isRequired,
    dateExpiration: propTypes.string,
    issueDate: propTypes.string,
    isFederal: propTypes.number.isRequired,
    createdBy: propTypes.shape({
      id: propTypes.number,
      firstName: propTypes.string,
      lastName: propTypes.string,
    }),
  }),
};
export default LicenseTR;
