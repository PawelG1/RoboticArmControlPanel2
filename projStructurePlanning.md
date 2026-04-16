# project structure planning
## Control Panel Project Structure uses the following clean architecture:
```
RoboticArmControlPanel (Solution)
│
├── 📁 ControlPanel.Domain (Class Library .NET)
│   ├── 📁 Entities
│   │   ├── Actuator.cs
│   │   ├── Servo.cs
│   │   └── StepperMotor.cs
│   ├── 📁 Exceptions
│   │   └── IncorrectTargetAngleParsedException.cs
│   └── 📁 Enums
│       └── ##OrderStatus.cs
│
├── 📁 ControlPanel.Application (Class Library .NET)
│   ├── 📁 Interfaces
│   │   ├── IProductRepository.cs
│   │   └── ICustomerRepository.cs
│   ├── 📁 UseCases
│   │   ├── GetAllProductsUseCase.cs
│   │   ├── CreateProductUseCase.cs
│   │   └── UpdateProductUseCase.cs
│   └── 📁 DTOs
│       └── ProductDto.cs
│
├── 📁 ControlPanel.Infrastructure (Class Library .NET)
│   ├── 📁 Persistence
│   │   ├── 📁 Entities
│   │   │   ├── ProductEntity.cs
│   │   │   └── CustomerEntity.cs
│   │   ├── AppDbContext.cs
│   │   ├── ProductRepository.cs
│   │   └── CustomerRepository.cs
│   ├── 📁 ExternalServices
│   │    └── EmailService.cs
│   ├── 📁Hardware
│       └── SerialPortController.cs 
│ 
└── 📁 ControlPanel.Presentation.WPF (WPF Application)
    ├── 📁 Views
    │   ├── ProductListView.xaml
    │   └── ProductListView.xaml.cs
    ├── 📁 ViewModels
    │   └── ProductListViewModel.cs
    ├── 📁 Commands
    │   └── RelayCommand.cs
    ├── App.xaml
    ├── App.xaml.cs
    └── MainWindow.xaml
```

```
┌─────────────────┐
│  PRESENTATION   │ ───┐
└─────────────────┘    │
                       ▼
┌─────────────────┐    ┌─────────────────┐
│ INFRASTRUCTURE  │───▶│   APPLICATION   │
└─────────────────┘    └─────────────────┘
                              │
                              ▼
                       ┌─────────────────┐
                       │     DOMAIN      │
                       └─────────────────┘
```