# Campus 360 - Comprehensive Campus Management System

[![.NET](https://img.shields.io/badge/.NET-8.0-purple)](https://dotnet.microsoft.com/)
[![Blazor](https://img.shields.io/badge/Blazor-WebAssembly-blue)](https://blazor.net/)
[![Supabase](https://img.shields.io/badge/Database-Supabase-green)](https://supabase.com/)
[![License](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

## 🎓 Overview

Campus 360 is a modern, comprehensive campus management system built with Blazor WebAssembly and powered by Supabase. It provides a unified platform for managing all aspects of educational institutions, from student enrollment to administrative tasks.

## ✨ Key Features

### 👨‍💼 Admin Panel
- **User Management**: Complete CRUD operations for students, teachers, and administrators
- **Department Management**: Organize and manage academic departments
- **Course Management**: Create, assign, and manage courses across departments
- **File Center**: Centralized file management with permissions and sharing
- **Calendar Management**: Event scheduling, bulk import, and academic calendar management
- **Automation Engine**: Rule-based automation with testing and logging capabilities
- **System Settings**: Global configuration and system preferences
- **Reports & Analytics**: Comprehensive reporting with data visualization
- **Audit Logs**: Complete activity tracking and system monitoring

### 👨‍🏫 Teacher Portal
- **Course Management**: Manage assigned courses and curriculum
- **Attendance Tracking**: Digital attendance with bulk operations
- **Assignment Management**: Create and distribute assignments
- **Grade Management**: Record and manage student grades
- **Notice Board**: Send announcements and notices to students
- **File Sharing**: Upload and share course materials
- **Calendar Integration**: Personal and course scheduling

### 👨‍🎓 Student Portal
- **Dashboard**: Personalized academic overview
- **Course Enrollment**: Browse and enroll in available courses
- **Attendance Monitoring**: View attendance records and statistics
- **Assignment Submission**: Submit assignments and track progress
- **Grade Tracking**: Monitor academic performance
- **Download Center**: Access course materials and resources
- **Personal Calendar**: Track academic events and deadlines
- **Notice Board**: Receive important announcements

## 🛠️ Technology Stack

### Frontend
- **Blazor WebAssembly** - Modern web framework using C#
- **Bootstrap 5** - Responsive UI framework
- **Modern CSS Grid/Flexbox** - Advanced layout systems

### Backend & Database
- **Supabase** - Backend-as-a-Service with PostgreSQL
- **PostgreSQL** - Robust relational database
- **Row Level Security (RLS)** - Advanced data security

### Development Tools
- **.NET 8.0** - Latest .NET framework
- **Entity Framework Core** - Object-relational mapping
- **Newtonsoft.Json** - JSON serialization
- **Npgsql** - PostgreSQL data provider

## 🚀 Getting Started

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)
- [Supabase Account](https://supabase.com/)

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/Abdullahkhan1010/Campus-360.git
   cd Campus-360
   ```

2. **Set up Supabase**
   - Create a new project in Supabase
   - Copy your project URL and anon key
   - Run the database schema scripts (located in the root directory)

3. **Configure the application**
   ```bash
   cp appsettings.sample.json Campus360/appsettings.json
   ```
   Update the configuration with your Supabase credentials:
   ```json
   {
     "Supabase": {
       "Url": "your-supabase-url",
       "Key": "your-anon-key"
     }
   }
   ```

4. **Build and run**
   ```bash
   cd Campus360
   dotnet restore
   dotnet run
   ```

5. **Access the application**
   - Open your browser and navigate to `https://localhost:5001`
   - Use the default admin credentials (see database setup scripts)

## 📁 Project Structure

```
Campus-360/
├── Campus360/                     # Main application
│   ├── Components/                # Reusable Blazor components
│   │   ├── Admin/                # Admin-specific components
│   │   ├── Calendar/             # Calendar components
│   │   ├── FileManagement/       # File management components
│   │   └── Teacher/              # Teacher-specific components
│   ├── Layout/                   # Application layouts
│   ├── Models/                   # Data models and DTOs
│   ├── Pages/                    # Blazor pages
│   │   ├── Admin/               # Admin panel pages
│   │   ├── Student/             # Student portal pages
│   │   └── Teacher/             # Teacher portal pages
│   ├── Services/                # Business logic and API services
│   └── wwwroot/                 # Static files and assets
├── Database Scripts/            # SQL scripts for database setup
└── Documentation/              # Project documentation
```

## 🗃️ Database Schema

The system uses a comprehensive PostgreSQL schema with the following main entities:

- **Users & Authentication**: User profiles, roles, permissions
- **Academic Structure**: Departments, courses, enrollments
- **Attendance System**: Attendance records and analytics
- **Assignment Management**: Assignments, submissions, grading
- **File Management**: File storage with permissions
- **Calendar System**: Events, schedules, notifications
- **Automation Engine**: Rules, triggers, and logs
- **System Settings**: Configuration and preferences

## 🔐 Security Features

- **Row Level Security (RLS)**: Database-level access control
- **Role-based Access Control**: Admin, Teacher, Student roles
- **Authentication**: Secure login with Supabase Auth
- **Data Validation**: Client and server-side validation
- **Audit Logging**: Complete activity tracking
- **File Security**: Permission-based file access

## 🎨 UI/UX Features

- **Responsive Design**: Mobile-first approach
- **Modern Interface**: Clean, professional design
- **Dark/Light Mode**: Theme switching capability
- **Accessibility**: WCAG compliance
- **Progressive Enhancement**: Works without JavaScript
- **Fast Loading**: Optimized performance

## 📊 Automation & Analytics

### Automation Engine
- Rule-based automation system
- Automated notifications and alerts
- Scheduled tasks and reminders
- Custom workflow creation

### Analytics & Reporting
- Student performance analytics
- Attendance tracking and trends
- Course enrollment statistics
- System usage metrics
- Custom report generation

## 🔧 Configuration

### Environment Variables
```env
SUPABASE_URL=your-supabase-url
SUPABASE_ANON_KEY=your-anon-key
ENVIRONMENT=Development|Production
```

### Application Settings
- Database connection settings
- Authentication configuration
- File upload limits
- Email service settings
- Automation rules configuration

## 🧪 Testing

The project includes comprehensive testing setup:

```bash
# Run unit tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

## 📈 Performance

- **Blazor WebAssembly**: Client-side execution for fast interactions
- **Lazy Loading**: Components loaded on demand
- **Caching Strategy**: Efficient data caching
- **Database Optimization**: Indexed queries and connection pooling
- **CDN Integration**: Static asset optimization

## 🛣️ Roadmap

### Phase 1 (Current)
- ✅ Core user management
- ✅ Basic academic features
- ✅ File management system
- ✅ Calendar integration

### Phase 2 (Planned)
- 📋 Advanced reporting dashboard
- 📱 Mobile app development
- 🔔 Real-time notifications
- 📊 Advanced analytics

### Phase 3 (Future)
- 🤖 AI-powered insights
- 📧 Email integration
- 💬 Chat system
- 🔗 Third-party integrations

## 🤝 Contributing

We welcome contributions! Please follow these steps:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 👥 Authors

- **Abdullah Khan** - *Initial work* - [@Abdullahkhan1010](https://github.com/Abdullahkhan1010)

## 🙏 Acknowledgments

- Thanks to the Blazor community for excellent documentation
- Supabase team for providing an amazing backend service
- Bootstrap team for the responsive framework
- All contributors who help improve this project

## 📞 Support

If you encounter any issues or have questions:

1. Check the [Issues](https://github.com/Abdullahkhan1010/Campus-360/issues) page
2. Create a new issue with detailed information
3. Contact: abdullah.khan1010@gmail.com

## 🔗 Links

- [Live Demo](https://campus-360-demo.netlify.app) (Coming Soon)
- [Documentation](https://github.com/Abdullahkhan1010/Campus-360/wiki)
- [API Reference](https://github.com/Abdullahkhan1010/Campus-360/blob/main/API.md)

---

<div align="center">
  <p>Built with ❤️ using Blazor and Supabase</p>
  <p>© 2025 Abdullah Khan. All rights reserved.</p>
</div>