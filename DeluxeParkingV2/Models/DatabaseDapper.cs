using Dapper;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using static Dapper.SqlMapper;

namespace DeluxeParkingV2.Models
{
    internal class DatabaseDapper
    {
        static string connString = "data source=.\\SQLEXPRESS; initial catalog = Parking10; persist security info = True; Integrated Security = True;";

        static int affectedRows = 0;
        public static List<Cities> GetAllCities()
        {
            const string sql = "SELECT * FROM Cities";
            using (var connection = new SqlConnection(connString))
            {
                return connection.Query<Cities>(sql).AsList();
            }
        }

        public static List<Cars> GetAllCars()
        {
            const string sql = "SELECT * FROM Cars";
            using (var connection = new SqlConnection(connString))
            {
                return connection.Query<Cars>(sql).AsList();
            }
        }

        public static List<ParkingHouses> GetAllParkingHouses()
        {
            const string sql = @"
            SELECT
                PH.Id,
                PH.HouseName,
                PH.CityId,
                STRING_AGG(PS.SlotNumber, ', ') AS SlotNumbers
            FROM ParkingHouses PH
            INNER JOIN ParkingSlots PS ON PH.Id = PS.ParkingHouseId
            GROUP BY PH.Id, PH.HouseName, PH.CityId";

            using (var connection = new SqlConnection(connString))
            {
                return connection.Query<ParkingHouses>(sql).AsList();
            }
        }

        public static List<ParkingSlots> GetAllSlots()
        {
            const string sql = "SELECT * FROM ParkingSlots";
            using (var connection = new SqlConnection(connString))
            {
                return connection.Query<ParkingSlots>(sql).AsList();
            }
        }

        public static int InsertCity(Cities city)
        {
            const string sql = "INSERT INTO Cities (CityName) VALUES (@CityName)";
            using (var connection = new SqlConnection(connString))
            {
                return connection.Execute(sql, city);
            }
        }

        public static int InsertCar(Cars car)
        {
            const string sql = "INSERT INTO Cars(Plate, Make, Color) VALUES(@Plate, @Make, @Color)";
            using (var connection = new SqlConnection(connString))
            {
                return connection.Execute(sql, car);
            }
        }
        public static int InsertParkingHouse(Models.ParkingHouses parkingHouses, Models.ParkingSlots slots, int[] outletSlots)
        {
            using (var connection = new SqlConnection(connString))
            {
                connection.Open();

                string parkingHouseSql = "INSERT INTO ParkingHouses(HouseName, CityId) OUTPUT INSERTED.Id VALUES (@HouseName, @CityId)";
                int parkingHouseId = connection.QuerySingle<int>(parkingHouseSql, new { HouseName = parkingHouses.HouseName, CityId = parkingHouses.CityId });

                slots.ParkingHouseId = parkingHouseId;

                string parkingSlotSql = $"INSERT INTO ParkingSlots(SlotNumber, ParkingHouseId) VALUES (@SlotNumber, @ParkingHouseId)";
                for (int i = 1; i <= slots.SlotNumber; i++)
                {
                    affectedRows += connection.Execute(parkingSlotSql, new { SlotNumber = i, ParkingHouseId = parkingHouseId });
                }

                string outletSql = "Update ParkingSlots SET ElectricOutlet = @OutletSlot WHERE SlotNumber = @SlotNumber AND ParkingHouseId = @ParkingHouseId";
                for (int i = 0; i < outletSlots.Length; i++)
                {
                    affectedRows += connection.Execute(outletSql, new { SlotNumber = i + 1, ParkingHouseId = parkingHouseId, OutletSlot = (outletSlots[i] == 0 ? 0 : 1) });
                }
            }

            return affectedRows;
        }

        public static int CarDrive(int carId)
        {
            string sql = $"UPDATE Cars Set ParkingSlotsId = NULL WHERE Id = {carId}";

            using (var connection = new SqlConnection(connString))
            {
                return affectedRows = connection.Execute(sql);
            }
        }
        public static List<Models.ParkingHouses> FreeSpots(int cityId)
        {
            List<Models.ParkingHouses> freeSlots = new List<Models.ParkingHouses>();

            string sql = @$"
            DECLARE @CityId INT = {cityId};
            SELECT
            PH.Id,
            PH.HouseName,
            PH.CityId,
            STRING_AGG(PS.SlotNumber, ', ') AS SlotNumbers
            FROM ParkingHouses PH
            JOIN ParkingSlots PS ON PH.Id = PS.ParkingHouseId
            LEFT JOIN Cars C ON PS.Id = C.ParkingSlotsId
            WHERE PH.CityId = @CityId AND C.ParkingSlotsId IS NULL
            GROUP BY PH.Id, PH.HouseName, PH.CityId;";

            using (var connection = new SqlConnection(connString))
            {
                return freeSlots = connection.Query<Models.ParkingHouses>(sql).ToList();
            }
        }
        public static List<Models.ParkingHouses> AllFreeSpots()
        {
            List<Models.ParkingHouses> allFreeSlots = new List<Models.ParkingHouses>();

            string sql = @"SELECT
            PH.Id,
            PH.HouseName,
            PH.CityId,
            STRING_AGG(PS.SlotNumber, ', ') AS SlotNumbers
            FROM ParkingHouses PH
            JOIN ParkingSlots PS ON PH.Id = PS.ParkingHouseId
            LEFT JOIN Cars C ON PS.Id = C.ParkingSlotsId
            WHERE C.ParkingSlotsId IS NULL
            GROUP BY PH.Id, PH.HouseName, PH.CityId;";

            using (var connection = new SqlConnection(connString))
            {
                return allFreeSlots = connection.Query<Models.ParkingHouses>(sql).ToList();
            }
        }
        public static List<Models.ParkingHouses> ParkedSpots()
        {
            List<Models.ParkingHouses> parkedSpots = new List<ParkingHouses>();

            string sql = @"SELECT
                            PH.HouseName AS ParkingHouse,
                            STRING_AGG(PS.SlotNumber, ', ') AS EmptySlots
                            FROM ParkingHouses PH
                            JOIN ParkingSlots PS ON PH.Id = PS.ParkingHouseId
                            LEFT JOIN Cars C ON PS.Id = C.ParkingSlotsId
                            WHERE PH.Id = @HouseId3 AND C.ParkedSlotsId IS NULL
                            GROUP BY PH.HouseName;";
            using (var connection = new SqlConnection(connString))
            {
                return parkedSpots = connection.Query<Models.ParkingHouses>(sql).ToList();
            }
        }
        public static int ParkCar(int carId, int houseId, int slotNumber)
        {
            string sql = @$"
            DECLARE @CarIdNew INT = {carId};
            DECLARE @HouseIdNew INT = {houseId};
            DECLARE @SlotNumberNew INT = {slotNumber};

            UPDATE Cars
            SET ParkingSlotsId = (
                SELECT PS.Id
                FROM ParkingSlots PS
                WHERE PS.ParkingHouseId = @HouseIdNew
                    AND PS.SlotNumber = @SlotNumberNew
            )
            WHERE Id = @CarIdNew;";

            using (var conn = new SqlConnection(connString))
            {
                affectedRows = conn.Execute(sql);
            }
            return affectedRows;
        }
        public static List<Models.ParkedCars> ShowParkedCars()
        {
            List<Models.ParkedCars> showCars = new List<Models.ParkedCars>();

            string sql = @"SELECT 
                            C.Id,
                            C.Plate,
                            C.Color,
                            PH.HouseName,
                            CONCAT(PS.SlotNumber, ' (', C.ParkingSlotsId, ')') AS ParkingSlot,
	                        PS.ElectricOutlet
                        FROM 
                            Cars C
                        INNER JOIN 
                            ParkingSlots PS ON PS.Id = C.ParkingSlotsId
                        INNER JOIN 
                            ParkingHouses PH ON PH.Id = PS.ParkingHouseId
                        GROUP BY 
                            C.Id,
                            C.Plate,
                            C.Color,
                            PH.HouseName,
                            PS.SlotNumber,
	                        PS.ElectricOutlet,
                            C.ParkingSlotsId;";
            using (var conn = new SqlConnection(connString))
            {
                return showCars = conn.Query<Models.ParkedCars>(sql).ToList();
            }
        }
        public static List<Models.SpotsElectricOutlet> ShowElectricOutlet() 
        {
            List<Models.SpotsElectricOutlet> showOutlet = new List<Models.SpotsElectricOutlet>();

            string sql = @"SELECT
						HouseName,
                        C.CityName,
                        STRING_AGG(SlotNumber,', ') WITHIN GROUP (ORDER BY SlotNumber) AS SlotNumber
                        FROM ParkingSlots PS
                        INNER JOIN ParkingHouses PH ON PS.ParkingHouseId = PH.Id
                        INNER JOIN Cities C ON PH.CityId = C.Id
                        WHERE ElectricOutlet = 1
                        GROUP BY HouseName, CityName
                        ORDER BY HouseName DESC;";
            using (var conn = new SqlConnection(connString))
            {
                return showOutlet = conn.Query<Models.SpotsElectricOutlet>(sql).ToList();
            }
        }
        public static List<Models.SpotsElectricOutlet> ShowElectricOutletTotal()
        {
            List<Models.SpotsElectricOutlet> showOutletTotal = new List<Models.SpotsElectricOutlet>();

            string sql = @"SELECT
                    C.CityName,
                    COUNT(SlotNumber) AS TotalAmount
                    FROM ParkingSlots PS
                    INNER JOIN ParkingHouses PH ON PS.ParkingHouseId = PH.Id
                    INNER JOIN Cities C ON PH.CityId = C.Id
                    WHERE ElectricOutlet = 1
                    GROUP BY CityName
                    ;";
            using (var conn = new SqlConnection(connString))
            {
                return showOutletTotal = conn.Query<Models.SpotsElectricOutlet>(sql).ToList();
            }
        }
    }
}
