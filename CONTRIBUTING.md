# Contributing to StroopApp

Thank you for considering contributing to **StroopApp**. Your contributions help improve the tool for researchers in psychology and neuroscience. Below are the guidelines to ensure a smooth and effective collaboration.

---

## **How Can I Contribute?**

### **1. Reporting Bugs**
- Use the [GitHub Issues](https://github.com/ELPN-Lab/StroopApp/issues) page to report bugs.
- Provide the following details:
  - A clear and concise description of the issue.
  - Steps to reproduce the bug.
  - Your environment (OS, .NET version, StroopApp version).
  - Screenshots or logs, if applicable.

### **2. Suggesting Enhancements**
- Open an issue with the **enhancement** label.
- Describe the feature or improvement you propose.
- Explain the use case and why it would benefit the community.

### **3. Submitting Code Changes**
1. **Fork the Repository**: Create a fork of the [StroopApp repository](https://github.com/ELPN-Lab/StroopApp).
2. **Create a Branch**: Use a descriptive name for your branch (e.g., `feature/add-multilingual-support` or `fix/timing-validation`).
3. **Make Your Changes**: Follow the coding guidelines below.
4. **Commit Your Changes**: Write clear and concise commit messages.
5. **Push to Your Fork**: Push your changes to your forked repository.
6. **Open a Pull Request (PR)**: Submit a PR to the `master` branch of the original repository. Ensure your PR:
   - References the related issue (if any).
   - Includes tests for new features or bug fixes.
   - Follows the existing code style and architecture (C#, WPF, MVVM).

### **4. Improving Documentation**
- Fix typos, clarify instructions, or add examples.
- Submit documentation changes via a Pull Request.

### **5. Translating the Interface**
- StroopApp supports English and French. To add or improve translations:
  - Edit the resource files in [/Resources](StroopApp/Resources).
  - Open a PR with your updates.

---

## **Development Setup**

### **Prerequisites**
- **.NET 8.0 SDK**: Required to build and run the application.
- **Visual Studio 2022**: Recommended for development.

### **Building the Project**
1. Clone the repository:
   ```bash
   git clone https://github.com/ELPN-Lab/StroopApp.git
   ```
2. Navigate to the project directory:
   ```bash
   cd StroopApp
   ```
3. Restore dependencies and build the project:
   ```bash
   dotnet restore
   dotnet build
   ```

### **Running Tests**
Execute the test suite to ensure your changes do not break existing functionality:
   ```bash
   dotnet test
   ```

---

## **Coding Guidelines**

### **General Principles**
- Follow the **MVVM (Model-View-ViewModel)** architecture.
- Write **unit tests** for new features and bug fixes.
- Document public methods and complex logic using XML comments.
- Keep commits **atomic** and descriptive.

### **Code Style**
- Use **PascalCase** for class names and method names.
- Use **camelCase** for local variables and private fields.
- Avoid long methods; break them into smaller, reusable functions.
- Use meaningful names for variables, methods, and classes.

### **Pull Request Guidelines**
- Ensure your PR addresses a single issue or feature.
- Include a clear description of the changes and their purpose.
- Reference any related issues or discussions.

---

## **Community and Support**

### **Discussions**
- Use [GitHub Discussions](https://github.com/ELPN-Lab/StroopApp/discussions) for questions, ideas, or general feedback.

### **Contact**
- For private inquiries, contact [mylan.bghn@gmail.com](mailto:mylan.bghn@gmail.com).

---

## **Code of Conduct**
This project adheres to a [Code of Conduct](CODE_OF_CONDUCT.md). By participating, you agree to uphold its standards.

---

## **Acknowledgments**
We appreciate all contributions, whether they are bug reports, feature requests, code changes, or documentation improvements. Your efforts help make StroopApp a better tool for the research community.
