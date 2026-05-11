using PKS4.Pr4.Models;

namespace PKS4.Pr4.Data;

public static class TouristGuideSeedData
{
    public static void Initialize(TouristGuideContext context)
    {
        var removedCity = context.Cities.FirstOrDefault(item => item.Name == "Екатеринбург");
        if (removedCity != null)
        {
            context.Attractions.RemoveRange(context.Attractions.Where(item => item.CityId == removedCity.Id));
            context.Cities.Remove(removedCity);
            context.SaveChanges();
        }

        var cities = new[]
        {
            new City
            {
                Name = "Москва",
                Region = "Центральный федеральный округ",
                Population = 13100000,
                History = "Москва - столица России. Город известен Кремлем, Красной площадью, музеями, театрами, парками и современной городской инфраструктурой.",
                CoatOfArmsUrl = string.Empty,
                PhotoUrl = "https://images.unsplash.com/photo-1513326738677-b964603b136d?auto=format&fit=crop&w=1200&q=80"
            },
            new City
            {
                Name = "Казань",
                Region = "Республика Татарстан",
                Population = 1318000,
                History = "Казань - крупный культурный и туристический центр. В городе сочетаются русские и татарские традиции, историческая архитектура и современные пространства.",
                CoatOfArmsUrl = string.Empty,
                PhotoUrl = "https://images.unsplash.com/photo-1578922746465-3a80a228f223?auto=format&fit=crop&w=1200&q=80"
            },
            new City
            {
                Name = "Санкт-Петербург",
                Region = "Северо-Западный федеральный округ",
                Population = 5600000,
                History = "Санкт-Петербург - культурная столица России, город дворцов, каналов, музеев и белых ночей. Исторический центр входит в список Всемирного наследия ЮНЕСКО.",
                CoatOfArmsUrl = string.Empty,
                PhotoUrl = "https://images.unsplash.com/photo-1556610961-2fecc5927173?auto=format&fit=crop&w=1200&q=80"
            },

            new City
            {
                Name = "Сочи",
                Region = "Краснодарский край",
                Population = 466000,
                History = "Сочи - главный курортный город России на берегу Черного моря. Он известен морскими пляжами, горами, санаториями и олимпийскими объектами.",
                CoatOfArmsUrl = string.Empty,
                PhotoUrl = "https://images.unsplash.com/photo-1507525428034-b723cf961d3e?auto=format&fit=crop&w=1200&q=80"
            },
            new City
            {
                Name = "Нижний Новгород",
                Region = "Нижегородская область",
                Population = 1210000,
                History = "Нижний Новгород расположен на слиянии Оки и Волги. Город известен кремлем, ярмаркой, купеческой историей и живописными набережными.",
                CoatOfArmsUrl = string.Empty,
                PhotoUrl = "https://images.unsplash.com/photo-1547448415-e9f5b28e570d?auto=format&fit=crop&w=1200&q=80"
            }
        };

        foreach (var cityData in cities)
        {
            var city = context.Cities.FirstOrDefault(item => item.Name == cityData.Name);
            if (city == null)
            {
                context.Cities.Add(cityData);
            }
            else
            {
                city.Region = cityData.Region;
                city.Population = cityData.Population;
                city.History = cityData.History;
                city.CoatOfArmsUrl = cityData.CoatOfArmsUrl;
                city.PhotoUrl = cityData.PhotoUrl;
            }
        }

        context.SaveChanges();

        var cityIds = context.Cities.ToDictionary(item => item.Name, item => item.Id);
        var attractions = new[]
        {
            new Attraction
            {
                Name = "Красная площадь",
                Description = "Главная площадь Москвы и одно из самых известных мест России.",
                History = "Площадь существует много веков и находится рядом с Кремлем.",
                WorkingHours = "Круглосуточно",
                VisitPrice = 0,
                PhotoUrl = "https://images.unsplash.com/photo-1531973576160-7125cd663d86?auto=format&fit=crop&w=1000&q=80",
                CityId = cityIds["Москва"]
            },
            new Attraction
            {
                Name = "Парк Зарядье",
                Description = "Современный парк рядом с Кремлем с красивыми видами на центр Москвы.",
                History = "Парк был открыт в 2017 году и быстро стал популярным туристическим местом.",
                WorkingHours = "10:00 - 22:00",
                VisitPrice = 0,
                PhotoUrl = "https://images.unsplash.com/photo-1520637836862-4d197d17c90a?auto=format&fit=crop&w=1000&q=80",
                CityId = cityIds["Москва"]
            },
            new Attraction
            {
                Name = "Казанский Кремль",
                Description = "Исторический комплекс и объект Всемирного наследия ЮНЕСКО.",
                History = "На территории находятся мечеть Кул-Шариф и Благовещенский собор.",
                WorkingHours = "08:00 - 22:00",
                VisitPrice = 0,
                PhotoUrl = "https://images.unsplash.com/photo-1578922746465-3a80a228f223?auto=format&fit=crop&w=1000&q=80",
                CityId = cityIds["Казань"]
            },
            new Attraction
            {
                Name = "Улица Баумана",
                Description = "Популярная пешеходная улица с кафе, сувенирами и историческими зданиями.",
                History = "Одна из самых известных улиц Казани.",
                WorkingHours = "Круглосуточно",
                VisitPrice = 0,
                PhotoUrl = "https://images.unsplash.com/photo-1526772662000-3f88f10405ff?auto=format&fit=crop&w=1000&q=80",
                CityId = cityIds["Казань"]
            },
            new Attraction
            {
                Name = "Эрмитаж",
                Description = "Один из крупнейших художественных музеев мира.",
                History = "Основная экспозиция находится в Зимнем дворце и связанных с ним зданиях.",
                WorkingHours = "11:00 - 20:00",
                VisitPrice = 500,
                PhotoUrl = "https://images.unsplash.com/photo-1556610961-2fecc5927173?auto=format&fit=crop&w=1000&q=80",
                CityId = cityIds["Санкт-Петербург"]
            },
            new Attraction
            {
                Name = "Петропавловская крепость",
                Description = "Историческое ядро Санкт-Петербурга на Заячьем острове.",
                History = "Крепость была заложена в 1703 году и считается началом истории города.",
                WorkingHours = "10:00 - 18:00",
                VisitPrice = 0,
                PhotoUrl = "https://images.unsplash.com/photo-1556610961-2fecc5927173?auto=format&fit=crop&w=1000&q=80",
                CityId = cityIds["Санкт-Петербург"]
            },

            new Attraction
            {
                Name = "Олимпийский парк",
                Description = "Комплекс спортивных объектов, построенных к зимней Олимпиаде 2014 года.",
                History = "После Олимпиады парк стал одной из главных туристических зон Сочи.",
                WorkingHours = "09:00 - 23:00",
                VisitPrice = 0,
                PhotoUrl = "https://images.unsplash.com/photo-1507525428034-b723cf961d3e?auto=format&fit=crop&w=1000&q=80",
                CityId = cityIds["Сочи"]
            },
            new Attraction
            {
                Name = "Дендрарий Сочи",
                Description = "Большой парк с коллекцией растений из разных стран.",
                History = "Дендрарий был основан в конце XIX века и остается одним из символов курортного города.",
                WorkingHours = "09:00 - 18:00",
                VisitPrice = 320,
                PhotoUrl = "https://images.unsplash.com/photo-1500530855697-b586d89ba3ee?auto=format&fit=crop&w=1000&q=80",
                CityId = cityIds["Сочи"]
            },
            new Attraction
            {
                Name = "Нижегородский кремль",
                Description = "Каменная крепость XVI века и главная историческая достопримечательность города.",
                History = "Кремль расположен на высоком берегу у слияния Оки и Волги.",
                WorkingHours = "10:00 - 18:00",
                VisitPrice = 0,
                PhotoUrl = "https://images.unsplash.com/photo-1547448415-e9f5b28e570d?auto=format&fit=crop&w=1000&q=80",
                CityId = cityIds["Нижний Новгород"]
            },
            new Attraction
            {
                Name = "Чкаловская лестница",
                Description = "Монументальная лестница с видом на Волгу.",
                History = "Построена в середине XX века и стала одним из самых узнаваемых мест города.",
                WorkingHours = "Круглосуточно",
                VisitPrice = 0,
                PhotoUrl = "https://images.unsplash.com/photo-1547448415-e9f5b28e570d?auto=format&fit=crop&w=1000&q=80",
                CityId = cityIds["Нижний Новгород"]
            }
        };

        foreach (var attractionData in attractions)
        {
            var attraction = context.Attractions.FirstOrDefault(item =>
                item.Name == attractionData.Name && item.CityId == attractionData.CityId);

            if (attraction == null)
            {
                context.Attractions.Add(attractionData);
            }
            else
            {
                attraction.Description = attractionData.Description;
                attraction.History = attractionData.History;
                attraction.WorkingHours = attractionData.WorkingHours;
                attraction.VisitPrice = attractionData.VisitPrice;
                attraction.PhotoUrl = attractionData.PhotoUrl;
            }
        }

        context.SaveChanges();
    }
}
