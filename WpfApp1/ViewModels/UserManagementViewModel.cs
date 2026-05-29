using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using WpfApp1.BLL;
using WpfApp1.DTO;
using WpfApp1.Helpers;

namespace WpfApp1.ViewModels
{
    public class UserManagementViewModel : BaseViewModel
    {
        private readonly UserService _userService = new();

        // -------------------------------------------------------
        // Collections (binding cho DataGrid / ComboBox)
        // -------------------------------------------------------
        public ObservableCollection<UserAccount> Users         { get; } = new();
        public ObservableCollection<UserRole>    Roles         { get; } = new();
        public ObservableCollection<Staff>       AvailableStaff { get; } = new();

        // -------------------------------------------------------
        // Tài khoản đang chọn
        // -------------------------------------------------------
        private UserAccount? _selectedUser;
        public UserAccount? SelectedUser
        {
            get => _selectedUser;
            set
            {
                SetField(ref _selectedUser, value);
                OnPropertyChanged(nameof(HasSelectedUser));
                LoadEditForm();
            }
        }
        public bool HasSelectedUser => _selectedUser != null;

        // -------------------------------------------------------
        // Form Tạo mới
        // -------------------------------------------------------
        private string _newUsername = string.Empty;
        public string NewUsername
        {
            get => _newUsername;
            set => SetField(ref _newUsername, value);
        }

        public string NewPassword     { get; set; } = string.Empty;  // set từ code-behind
        public string SelectedStaffId { get; set; } = string.Empty;

        private int _selectedRoleId;
        public int SelectedRoleId
        {
            get => _selectedRoleId;
            set => SetField(ref _selectedRoleId, value);
        }

        // -------------------------------------------------------
        // Form Chỉnh sửa
        // -------------------------------------------------------
        private string _editUsername = string.Empty;
        public string EditUsername
        {
            get => _editUsername;
            set => SetField(ref _editUsername, value);
        }

        private int _editRoleId;
        public int EditRoleId
        {
            get => _editRoleId;
            set => SetField(ref _editRoleId, value);
        }

        private bool _editIsActive;
        public bool EditIsActive
        {
            get => _editIsActive;
            set => SetField(ref _editIsActive, value);
        }

        // -------------------------------------------------------
        // Form Đổi mật khẩu (set từ code-behind)
        // -------------------------------------------------------
        public string ChangeCurrent { get; set; } = string.Empty;
        public string ChangeNew     { get; set; } = string.Empty;
        public string ChangeConfirm { get; set; } = string.Empty;

        // -------------------------------------------------------
        // Status bar
        // -------------------------------------------------------
        private string _status = string.Empty;
        public string Status
        {
            get => _status;
            set => SetField(ref _status, value);
        }

        // -------------------------------------------------------
        // Commands
        // -------------------------------------------------------
        public ICommand LoadCommand           { get; }
        public ICommand CreateUserCommand     { get; }
        public ICommand UpdateUserCommand     { get; }
        public ICommand DeactivateUserCommand { get; }
        public ICommand ChangePasswordCommand { get; }

        public UserManagementViewModel()
        {
            LoadCommand           = new RelayCommand(Load);
            CreateUserCommand     = new RelayCommand(ExecuteCreate);
            UpdateUserCommand     = new RelayCommand(ExecuteUpdate,     () => HasSelectedUser);
            DeactivateUserCommand = new RelayCommand(ExecuteDeactivate, () => HasSelectedUser);
            ChangePasswordCommand = new RelayCommand(ExecuteChangePwd,  () => HasSelectedUser);

            Load();
        }

        // -------------------------------------------------------
        private void Load()
        {
            try
            {
                Users.Clear();
                foreach (var u in _userService.GetAllUsers())
                    Users.Add(u);

                Roles.Clear();
                foreach (var r in _userService.GetAllRoles())
                    Roles.Add(r);

                AvailableStaff.Clear();
                foreach (var s in _userService.GetStaffWithoutAccount())
                    AvailableStaff.Add(s);

                Status = $"Tải thành công – {Users.Count} tài khoản.";
            }
            catch (Exception ex)
            {
                Status = ExceptionHandler.Handle(ex, "UserManagementViewModel.Load");
            }
        }

        private void ExecuteCreate()
        {
            try
            {
                _userService.CreateUser(new CreateUserDto
                {
                    Username = NewUsername,
                    Password = NewPassword,
                    StaffID  = SelectedStaffId,
                    RoleID   = SelectedRoleId,
                });
                Status = "✅ Tạo tài khoản thành công!";
                NewUsername = string.Empty;
                Load();
            }
            catch (Exception ex)
            {
                Status = ExceptionHandler.Handle(ex, "UserManagementViewModel.Create");
            }
        }

        private void ExecuteUpdate()
        {
            try
            {
                _userService.UpdateUser(new UpdateUserDto
                {
                    AccountID = SelectedUser!.AccountID,
                    Username  = EditUsername,
                    RoleID    = EditRoleId,
                    IsActive  = EditIsActive,
                });
                Status = "✅ Cập nhật tài khoản thành công!";
                Load();
            }
            catch (Exception ex)
            {
                Status = ExceptionHandler.Handle(ex, "UserManagementViewModel.Update");
            }
        }

        private void ExecuteDeactivate()
        {
            var confirm = MessageBox.Show(
                $"Vô hiệu hóa tài khoản '{SelectedUser?.Username}'?",
                "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                _userService.DeactivateUser(SelectedUser!.AccountID);
                Status = "✅ Đã vô hiệu hóa tài khoản.";
                Load();
            }
            catch (Exception ex)
            {
                Status = ExceptionHandler.Handle(ex, "UserManagementViewModel.Deactivate");
            }
        }

        private void ExecuteChangePwd()
        {
            try
            {
                _userService.ChangePassword(new ChangePasswordDto
                {
                    AccountID       = SelectedUser!.AccountID,
                    CurrentPassword = ChangeCurrent,
                    NewPassword     = ChangeNew,
                    ConfirmPassword = ChangeConfirm,
                });
                Status = "✅ Đổi mật khẩu thành công!";
            }
            catch (Exception ex)
            {
                Status = ExceptionHandler.Handle(ex, "UserManagementViewModel.ChangePwd");
            }
        }

        private void LoadEditForm()
        {
            if (_selectedUser == null) return;
            EditUsername = _selectedUser.Username;
            EditRoleId   = _selectedUser.RoleID;
            EditIsActive = _selectedUser.IsActive;
        }
    }
}