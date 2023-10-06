using Yellowbrick.Data;
using Yellowbrick.Data.Providers;
using Yellowbrick.Models;
using Yellowbrick.Models.Requests.Licenses;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Yellowbrick.Services.Interfaces;
using License = Yellowbrick.Models.Domain.License.License;


namespace Yellowbrick.Services
{
    public class LicenseService : ILicenseService
    {
        IDataProvider _data = null;
        IMapBaseUser _mapBaseUser = null;
        ILookUpService _lookUp = null;

        public LicenseService(IDataProvider data, IMapBaseUser mapBaseUser, ILookUpService lookUp)
        {
            _data = data;
            _mapBaseUser = mapBaseUser;
            _lookUp = lookUp;
        }

        public int Create(LicenseAddRequest model, int userId)
        {
            int id = 0;
            string procName = "[dbo].[Licenses_Insert]";
            _data.ExecuteNonQuery(procName, inputParamMapper: delegate (SqlParameterCollection col)
            {
                AddCommonParams(model, col);
                col.AddWithValue("@UserId", userId);

                SqlParameter idOut = new SqlParameter("@Id", System.Data.SqlDbType.Int);
                idOut.Direction = ParameterDirection.Output;
                col.Add(idOut);
            }, returnParameters: delegate (SqlParameterCollection returnCollection)
            {
                object oId = returnCollection["@Id"].Value;
                int.TryParse(oId.ToString(), out id);
            });

            return id;
        }
        
        public License Get(int id)
        {

            License license = null;
            _data.ExecuteCmd("[dbo].[Licenses_SelectBy_Id]", delegate (SqlParameterCollection col)
            {
                col.AddWithValue("@Id", id);
            }, delegate (IDataReader reader, short set)
            {
                int index = 0;
                license = MapSingleLicense(reader, ref index);

            });
            return license;

        }

        public void Delete(int id)
        {
            string procName = "[Licenses_DeleteById]";
            _data.ExecuteNonQuery(procName,

            inputParamMapper: delegate (SqlParameterCollection col)
            {
                col.AddWithValue("@Id", id);
            },
            returnParameters: null);
        }

        public Paged<License> SelectByCreatedBy(int createdByUserId, int pageIndex, int pageSize)
        {
            Paged<License> pagedResult = null;

            List<License> result = null;

            int totalCount = 0;

            string procName = "[dbo].[Licenses_Select_ByCreatedBy]";
            _data.ExecuteCmd(procName, inputParamMapper: delegate (SqlParameterCollection col)
            {
                col.AddWithValue("@UserId", createdByUserId);
                col.AddWithValue("@PageIndex", pageIndex);
                col.AddWithValue("@PageSize", pageSize);
            },
            singleRecordMapper: delegate (IDataReader reader, short set)
            {
                License license = new License();

                int index = 0;
                license = MapSingleLicense(reader, ref index);

                if (totalCount == 0)
                {
                    totalCount = reader.GetSafeInt32(index);
                }

                if (result == null)
                {
                    result = new List<License>();
                }

                result.Add(license);
            });

            if (result != null)
            {
                pagedResult = new Paged<License>(result, pageIndex, pageSize, totalCount);
            }

            return pagedResult;
        }

        public Paged<License> LicensePagination(int pageIndex, int pageSize)
        {

            Paged<License> pagedResult = null;

            List<License> result = null;

            int totalCount = 0;

            string procName = "[dbo].[Licenses_SelectAll]";
            _data.ExecuteCmd(procName, inputParamMapper: delegate (SqlParameterCollection col)
            {
                col.AddWithValue("@PageIndex", pageIndex);
                col.AddWithValue("@PageSize", pageSize);
            },
            singleRecordMapper: delegate (IDataReader reader, short set)
            {
                License license = new License();

                int index = 0;
                license = MapSingleLicense(reader, ref index);

                if (totalCount == 0)
                {
                    totalCount = reader.GetSafeInt32(index);
                }

                if (result == null)
                {
                    result = new List<License>();
                }

                result.Add(license);
            }

            );
            if (result != null)
            {
                pagedResult = new Paged<License>(result, pageIndex, pageSize, totalCount);
            }

            return pagedResult;
        }

        public void Update(LicenseUpdateRequest model, int userId)
        {
            string procName = "[dbo].[Licenses_Update]";
            _data.ExecuteNonQuery(procName,

                inputParamMapper: delegate (SqlParameterCollection col)
                {
                    AddCommonParams(model, col);
                    col.AddWithValue("@UserId", userId);
                    col.AddWithValue("@Id", model.Id);
                },
            returnParameters: null);
        }

        public Paged<License> Search(int pageIndex, int pageSize, string query)
        {
            Paged<License> pagedList = null;
            List<License> list = null;
            int totalCount = 0;
            _data.ExecuteCmd("[dbo].[Licenses_PaginatedSearch]"
                , (col) =>
                {
                    col.AddWithValue("@PageIndex", pageIndex);
                    col.AddWithValue("@PageSize", pageSize);
                    col.AddWithValue("@Query", query);
                }, (reader, recordSetIndex) =>
                {
                    int startingIndex = 0;

                    License license = MapSingleLicense(reader, ref startingIndex );

                    if (totalCount == 0)
                    {
                        totalCount = reader.GetSafeInt32(startingIndex++);
                    }
                    if (list == null)
                    {
                        list = new List<License>();
                    }
                    list.Add(license);
                });

            if (list != null)
            {
                pagedList = new Paged<License>(list, pageIndex, pageSize, totalCount);
            }
            return pagedList;
        }

        private static void AddCommonParams(LicenseAddRequest model, SqlParameterCollection col)
        {
            col.AddWithValue("@LicenseTypeId", model.LicenseTypeId);
            col.AddWithValue("@LicenseStateId", model.LicenseStateId);
            col.AddWithValue("@LicenseNumber", model.LicenseNumber);
            col.AddWithValue("@DateExpiration", model.DateExpiration);
            col.AddWithValue("@IssueDate", model.IssueDate);
            col.AddWithValue("@IsActive", model.IsActive);
            col.AddWithValue("@IsFederal", model.IsFederal);
        }

        public License MapSingleLicense(IDataReader reader, ref int startingIndex)
        {
            License license = new License();          
            license.Id = reader.GetSafeInt32(startingIndex++);
            license.LicenseType = _lookUp.MapSingleLookUp(reader, ref startingIndex);
            license.LicenseState = _lookUp.MapSingleLookUp(reader, ref startingIndex);
            license.LicenseNumber = reader.GetSafeString(startingIndex++);
            license.DateExpiration = reader.GetSafeDateTime(startingIndex++);
            license.IssueDate = reader.GetSafeDateTime(startingIndex++);
            license.IsActive = reader.GetSafeBool(startingIndex++);
            license.IsFederal = reader.GetSafeInt32(startingIndex++);
            license.CreatedBy = _mapBaseUser.MapBaseUser(reader, ref startingIndex);
            license.DateCreated = reader.GetSafeDateTime(startingIndex++);
            return license;
        }
    }
}

