using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Interactivity;
using AircraftApp.Models;
using Avalonia.Platform.Storage;

namespace AircraftApp.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly IRunway _runway;
        private readonly Window _mainWindow;
        private Aircraft? _currentAircraft;
        private Assembly? _loadedAssembly;
        private Type? _selectedType;
        private MethodInfo? _selectedMethod;
        private object? _instance;
        private string _status = string.Empty;
        private string _result = string.Empty;
        private string _assemblyPath = string.Empty;
        private double _runwayLength = 1500;
        private ObservableCollection<Type> _availableTypes = new();
        private ObservableCollection<MethodInfo> _availableMethods = new();
        private ObservableCollection<ParameterViewModel> _parameters = new();
        private ObservableCollection<Aircraft> _aircraftList = new();
        private ObservableCollection<ParameterViewModel> _constructorParameters = new();

        private RelayCommand _takeOffCommand;
        private RelayCommand _landCommand;

        public MainViewModel(Window mainWindow)
        {
            _mainWindow = mainWindow;
            _runway = new Runway("Main Runway");
            UpdateStatus();

            // Команды для управления ВПП
            _takeOffCommand = new RelayCommand(ExecuteTakeOff, () => _currentAircraft != null);
            _landCommand = new RelayCommand(ExecuteLand, () => _currentAircraft != null);

            // Команды для создания воздушных судов
            CreateAirplaneCommand = new RelayCommand(() => CreateAircraft<Airplane>(_runwayLength));
            CreateHelicopterCommand = new RelayCommand(() => CreateAircraft<Helicopter>());

            // Команды для рефлексии
            LoadAssemblyCommand = new RelayCommand(ExecuteLoadAssembly);
            ExecuteMethodCommand = new RelayCommand(ExecuteMethod);
        }

        public string Status
        {
            get => _status;
            set => this.RaiseAndSetIfChanged(ref _status, value);
        }

        public string Result
        {
            get => _result;
            set => this.RaiseAndSetIfChanged(ref _result, value);
        }

        public string AssemblyPath
        {
            get => _assemblyPath;
            set => this.RaiseAndSetIfChanged(ref _assemblyPath, value);
        }

        public double RunwayLength
        {
            get => _runwayLength;
            set => this.RaiseAndSetIfChanged(ref _runwayLength, value);
        }

        public ObservableCollection<Aircraft> AircraftList
        {
            get => _aircraftList;
            set => this.RaiseAndSetIfChanged(ref _aircraftList, value);
        }

        public Aircraft? CurrentAircraft
        {
            get => _currentAircraft;
            set
            {
                if (this.RaiseAndSetIfChanged(ref _currentAircraft, value))
                {
                    _takeOffCommand.RaiseCanExecuteChanged();
                    _landCommand.RaiseCanExecuteChanged();
                    if (value != null)
                    {
                        Status = $"Выбран {value.Type}";
                    }
                }
            }
        }

        public ObservableCollection<Type> AvailableTypes
        {
            get => _availableTypes;
            set => this.RaiseAndSetIfChanged(ref _availableTypes, value);
        }

        public ObservableCollection<MethodInfo> AvailableMethods
        {
            get => _availableMethods;
            set => this.RaiseAndSetIfChanged(ref _availableMethods, value);
        }

        public ObservableCollection<ParameterViewModel> Parameters
        {
            get => _parameters;
            set => this.RaiseAndSetIfChanged(ref _parameters, value);
        }

        public ObservableCollection<ParameterViewModel> ConstructorParameters
        {
            get => _constructorParameters;
            set => this.RaiseAndSetIfChanged(ref _constructorParameters, value);
        }

        public Type? SelectedType
        {
            get => _selectedType;
            set
            {
                if (this.RaiseAndSetIfChanged(ref _selectedType, value))
                {
                    _instance = null;
                    UpdateAvailableMethods();
                    UpdateConstructorParameters();
                }
            }
        }

        public MethodInfo? SelectedMethod
        {
            get => _selectedMethod;
            set
            {
                if (this.RaiseAndSetIfChanged(ref _selectedMethod, value))
                {
                    UpdateParameters();
                }
            }
        }

        public ICommand TakeOffCommand => _takeOffCommand;
        public ICommand LandCommand => _landCommand;
        public ICommand CreateAirplaneCommand { get; }
        public ICommand CreateHelicopterCommand { get; }
        public ICommand LoadAssemblyCommand { get; }
        public ICommand ExecuteMethodCommand { get; }

        private void CreateAircraft<T>(double runwayLength = 0) where T : Aircraft
        {
            try
            {
                Aircraft? newAircraft = null;
                if (typeof(T) == typeof(Airplane))
                {
                    newAircraft = new Airplane(runwayLength);
                    Status = $"Создан самолет. Длина ВПП: {runwayLength}м";
                }
                else if (typeof(T) == typeof(Helicopter))
                {
                    newAircraft = new Helicopter();
                    Status = "Создан вертолет";
                }

                if (newAircraft != null)
                {
                    newAircraft.StatusChanged += OnAircraftStatusChanged;
                    AircraftList.Add(newAircraft);
                    CurrentAircraft = newAircraft;
                }
            }
            catch (Exception ex)
            {
                Status = $"Ошибка создания воздушного судна: {ex.Message}";
            }
        }

        private void OnAircraftStatusChanged(object? sender, string status)
        {
            Status = status;
        }

        private void ExecuteTakeOff()
        {
            try
            {
                if (_currentAircraft != null)
                {
                    if (_currentAircraft.TakeOff())
                    {
                        _runway.TakeOff();
                    }
                }
            }
            catch (Exception ex)
            {
                Status = $"Ошибка: {ex.Message}";
            }
        }

        private void ExecuteLand()
        {
            try
            {
                if (_currentAircraft != null)
                {
                    _currentAircraft.Land();
                    _runway.Land();
                }
            }
            catch (Exception ex)
            {
                Status = $"Ошибка: {ex.Message}";
            }
        }

        private void UpdateStatus()
        {
            Status = _runway.GetStatus();
        }

        private async void ExecuteLoadAssembly()
        {
            try
            {
                var files = await _mainWindow.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                {
                    Title = "Выберите DLL файл",
                    AllowMultiple = false,
                    FileTypeFilter = new[] 
                    { 
                        new FilePickerFileType("DLL файлы")
                        {
                            Patterns = new[] { "*.dll" }
                        }
                    }
                });

                if (files.Count > 0)
                {
                    var file = files[0];
                    AssemblyPath = file.Path.LocalPath;
                    
                    _loadedAssembly = Assembly.LoadFrom(AssemblyPath);
                    
                    // Получаем только нужные классы
                    var types = new List<Type>
                    {
                        typeof(Aircraft),
                        typeof(Airplane),
                        typeof(Helicopter)
                    };
                    
                    AvailableTypes.Clear();
                    foreach (var type in types)
                    {
                        AvailableTypes.Add(type);
                    }

                    if (types.Any())
                    {
                        SelectedType = types[0];
                        Result = "Загружены классы: Aircraft, Airplane, Helicopter";
                    }
                    else
                    {
                        Result = "Не удалось загрузить классы";
                    }
                }
            }
            catch (Exception ex)
            {
                Result = $"Ошибка загрузки сборки: {ex.Message}";
            }
        }

        private void UpdateAvailableMethods()
        {
            if (_selectedType == null) return;

            // Получаем все публичные методы класса
            var methods = _selectedType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => !m.IsSpecialName && !m.IsAbstract)
                .ToList();
            
            AvailableMethods.Clear();
            foreach (var method in methods)
            {
                AvailableMethods.Add(method);
            }

            if (methods.Any())
            {
                SelectedMethod = methods[0];
                Result = $"Найдено {methods.Count} методов в классе {_selectedType.Name}";
            }
            else
            {
                Result = $"В классе {_selectedType.Name} не найдено публичных методов";
            }
        }

        private void UpdateParameters()
        {
            Parameters.Clear();
            
            if (_selectedMethod == null) return;

            foreach (var parameter in _selectedMethod.GetParameters())
            {
                Parameters.Add(new ParameterViewModel
                {
                    Name = parameter.Name,
                    ParameterType = parameter.ParameterType
                });
            }
        }

        private void UpdateConstructorParameters()
        {
            ConstructorParameters.Clear();
            if (_selectedType == null) return;
            // Берём первый публичный конструктор
            var ctor = _selectedType.GetConstructors().OrderByDescending(c => c.GetParameters().Length).FirstOrDefault();
            if (ctor != null)
            {
                foreach (var param in ctor.GetParameters())
                {
                    ConstructorParameters.Add(new ParameterViewModel
                    {
                        Name = param.Name,
                        ParameterType = param.ParameterType
                    });
                }
            }
        }

        private void ExecuteMethod()
        {
            try
            {
                if (_selectedMethod == null)
                {
                    Result = "Пожалуйста, выберите метод";
                    return;
                }

                if (_selectedType == null)
                {
                    Result = "Пожалуйста, выберите класс";
                    return;
                }

                if (_instance == null)
                {
                    // Создаём экземпляр с параметрами конструктора
                    var ctor = _selectedType.GetConstructors().OrderByDescending(c => c.GetParameters().Length).FirstOrDefault();
                    if (ctor == null)
                    {
                        Result = "У выбранного класса нет публичных конструкторов.";
                        return;
                    }
                    var ctorParams = ctor.GetParameters();
                    var values = new object[ctorParams.Length];
                    for (int i = 0; i < ctorParams.Length; i++)
                    {
                        var paramVm = ConstructorParameters.FirstOrDefault(p => p.Name == ctorParams[i].Name);
                        if (paramVm == null)
                        {
                            Result = $"Не задан параметр конструктора: {ctorParams[i].Name}";
                            return;
                        }
                        values[i] = Convert.ChangeType(paramVm.Value, ctorParams[i].ParameterType);
                    }
                    _instance = ctor.Invoke(values);
                    if (_instance == null)
                    {
                        Result = "Не удалось создать экземпляр класса";
                        return;
                    }
                }

                var parameters = Parameters
                    .Select(p => Convert.ChangeType(p.Value, p.ParameterType))
                    .ToArray();
                
                var result = _selectedMethod.Invoke(_instance, parameters);
                Result = result?.ToString() ?? "Метод успешно выполнен";
            }
            catch (Exception ex)
            {
                Result = $"Ошибка выполнения метода: {ex.Message}";
            }
        }
    }

    public class ParameterViewModel : ViewModelBase
    {
        private string _name = string.Empty;
        private Type _parameterType = typeof(string);
        private string _value = string.Empty;

        public string Name
        {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }

        public Type ParameterType
        {
            get => _parameterType;
            set => this.RaiseAndSetIfChanged(ref _parameterType, value);
        }

        public string Value
        {
            get => _value;
            set => this.RaiseAndSetIfChanged(ref _value, value);
        }
    }
}
