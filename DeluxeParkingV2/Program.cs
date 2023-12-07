using DeluxeParkingV2.Models;
using System;
using System.Collections.Generic;

namespace DeluxeParkingV2
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            while (true)
            {
                PrintMenu();

                var key = Console.ReadKey();

                switch (key.KeyChar)
                {
                    case 'a':
                        ShowCars();
                        break;
                    case 'b':
                        InsertCar();
                        break;
                    case 'c':
                        InsertCity();
                        break;
                    case 'd':
                        ShowFreeParkingSpots();
                        break;
                    case 'e':
                        ShowParkingHouses();
                        break;
                    case 'f':
                        ShowCities();
                        break;
                    case 'g':
                        ParkCar();
                        break;
                    case 'h':
                        DriveCar();
                        break;
                    case 'i':
                        InsertParkingHouse();
                        break;
                    case 'j':
                        ShowAllParkedCars();
                        break;
                    case 'k':
                        ShowElectricOutlet();
                        break;
                    case 't':
                        ShowAllFreeParkingSpots();
                        break;
                }
                Console.Read();
                Console.Clear();
            }
        }

        private static void PrintMenu()
        {
            Console.WriteLine("A. Show Cars");
            Console.WriteLine("B. Insert Car");
            Console.WriteLine("C. Insert City");
            Console.WriteLine("D. Show Free-ParkingSpots (Specific House)");
            Console.WriteLine("E. Show ParkingHouse");
            Console.WriteLine("F. Show Cities");
            Console.WriteLine("G. Park Car");
            Console.WriteLine("H. Drive Car");
            Console.WriteLine("I. Insert ParkingHouse");
            Console.WriteLine("J. Show All Parked Cars");
            Console.WriteLine("K. Show All Electric Outlets");
            Console.WriteLine("T. Test");
        }

        private static void ParkCar()
        {
            ShowAllFreeParkingSpots();
            Console.WriteLine();
            ShowCars();

            int carIdToPark = GetIntegerInput("CarId");

            int houseIdToPark = GetIntegerInput("HouseId");

            int slotNumberToPark = GetIntegerInput("SlotNumber");

            int affectedRowsParkCar = DatabaseDapper.ParkCar(carIdToPark, houseIdToPark, slotNumberToPark);
        }


        private static void ShowFreeParkingSpots()
        {
            ShowCities();
            int cityId = GetIntegerInput("CityId");
            List <Models.ParkingHouses> freeSlots = DatabaseDapper.FreeSpots(cityId);
            foreach (Models.ParkingHouses freeSlot in freeSlots)
            {
                Console.WriteLine($"{freeSlot.Id}\t{freeSlot.HouseName}\t{freeSlot.CityId}\t{freeSlot.SlotNumbers}");
            }
        }

        private static void ShowAllParkedCars()
        {
            List<Models.ParkedCars> showCars = DatabaseDapper.ShowParkedCars();
            foreach(Models.ParkedCars car in showCars)
            {
                Console.WriteLine($"{car.Id}\t{car.Plate}\t{car.Color}\t{car.HouseName}\t{car.ParkingSlot}\t{(car.ElectricOutlet == 1 ? "Yes" : "No")}");
            }
        }

        private static void ShowElectricOutlet()
        {
            List<Models.SpotsElectricOutlet> showOutlet = DatabaseDapper.ShowElectricOutlet();
            foreach(Models.SpotsElectricOutlet sp in showOutlet)
            {
                Console.WriteLine($"{sp.CityName}\t{sp.HouseName}: {sp.SlotNumber}");
            }
            Console.WriteLine();
            List<Models.SpotsElectricOutlet> showTotalOutlets = DatabaseDapper.ShowElectricOutletTotal();
            foreach(Models.SpotsElectricOutlet show in showTotalOutlets)
            {
                Console.WriteLine($"{show.CityName}: {show.TotalAmount}");
            }
        }

        private static void ShowAllFreeParkingSpots()
        {
            List<Models.ParkingHouses> allFreeSlots = DatabaseDapper.AllFreeSpots();
            foreach(Models.ParkingHouses allFreeSlot in allFreeSlots)
            {
                Console.WriteLine($"{allFreeSlot.Id}\t{allFreeSlot.HouseName}\t{allFreeSlot.CityId}\t{allFreeSlot.SlotNumbers}");
            }
        }
        private static void DriveCar()
        {
            ShowCars();
            int carId = GetIntegerInput("CarId");
            int affectedRowsDriveCar = DatabaseDapper.CarDrive(carId);
        }

        private static void ShowCars()
        {
            List<Models.Cars> cars = DatabaseDapper.GetAllCars();
            foreach (Models.Cars car in cars)
            {
                Console.WriteLine($"{car.Id}\t{car.Plate}\t{car.Make}\t{car.Color}\t{car.ParkingSlotsId}");
            }
        }

        private static void InsertCar()
        {
            ShowCars();
            Models.Cars cars1 = new Models.Cars();
            string plate = GenerateRegistrationNumber();
            Console.WriteLine($"Plate: {plate}");
            Console.Write("Make: ");
            string make = Console.ReadLine();
            Console.Write("Color: ");
            string color = Console.ReadLine();
            cars1.Make = make;
            cars1.Plate = plate;
            cars1.Color = color;
            int affectedRows1 = DatabaseDapper.InsertCar(cars1);
        }

        private static void InsertCity()
        {
            ShowCities();
            Models.Cities cities1 = new Models.Cities();
            Console.Write("Name: ");
            string cityName = Console.ReadLine();
            cities1.CityName = cityName;
            int affectedRows2 = DatabaseDapper.InsertCity(cities1);
        }

        private static void ShowParkingHouses()
        {
            List<ParkingHouses> parkingHouses = DatabaseDapper.GetAllParkingHouses();
            foreach (ParkingHouses parkingHouse in parkingHouses)
            {
                Console.WriteLine($"{parkingHouse.Id}\t{parkingHouse.HouseName}\t{parkingHouse.CityId}\t{parkingHouse.SlotNumbers}");
            }
        }

        private static void ShowCities()
        {
            List<Models.Cities> cities = DatabaseDapper.GetAllCities();

            foreach (Models.Cities c in cities)
            {
                Console.WriteLine($"{c.Id}\t{c.CityName}");
            }
        }

        private static void InsertParkingHouse()
        {
            Models.ParkingHouses parkingHouse1 = new Models.ParkingHouses();
            Models.ParkingSlots parkingSlot1 = new Models.ParkingSlots();
            Console.Write("House Name: ");
            string houseName = Console.ReadLine();
            Console.Write("CityId: ");
            int cityId = int.Parse(Console.ReadLine());
            Console.Write("Amount of slots: ");
            int slotsAmount = int.Parse(Console.ReadLine());
            Console.Write("How many slots with electric outlet: ");
            int outletAmount = int.Parse(Console.ReadLine());
            int[] outletSlots = new int[slotsAmount];
            for(int i = 1; i <= outletAmount; i++)
            {
                Console.Write("Slot: ");
                int slotNumber = int.Parse(Console.ReadLine());
                outletSlots[i] = slotNumber;
            }
            parkingHouse1.HouseName = houseName;
            parkingHouse1.CityId = cityId;
            parkingSlot1.SlotNumber = slotsAmount;
            int affectedRows3 = DatabaseDapper.InsertParkingHouse(parkingHouse1, parkingSlot1, outletSlots);
        }
        private static int GetIntegerInput(string prompt)
        {
            int userInput;

            while (true)
            {
                Console.Write(prompt + ": ");
                string input = Console.ReadLine();

                if (int.TryParse(input, out userInput))
                {
                    return userInput;
                }
                else
                {
                    Console.WriteLine("Invalid input. Please enter a valid numeric value.");
                }
            }
        }
        private static string GenerateRegistrationNumber()
        {

            Random rnd = new Random();

            string registrationNumber = string.Empty;

            for (int i = 0; i < 3; i++)
            {
                char randomLetter = (char)('A' + rnd.Next(0, 26));
                registrationNumber += randomLetter;
            }

            for (int i = 0; i < 3; i++)
            {
                char randomNumber = (char)('0' + rnd.Next(0, 10));
                registrationNumber += randomNumber;
            }
            return registrationNumber;
        }
    }
}
