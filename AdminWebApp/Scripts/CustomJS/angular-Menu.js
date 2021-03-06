﻿function toastrMsg(msg, method) {
    toastr.options = {
        "positionClass": "toast-bottom-right",
        "preventDuplicates": true,
        "showDuration": "300",
        "hideDuration": "1000",
        "timeOut": "5000",
        "extendedTimeOut": "1000",
        "showEasing": "swing",
        "hideEasing": "linear",
        "showMethod": "fadeIn",
        "hideMethod": "fadeOut"
    };
    return toastr[method](msg);
}
var app = angular.module("myApp", []);
app.controller("myCtrl", function ($scope, $http) {
    $scope.Status = true;
    $scope.Test = "Active";

    $scope.GetValue = function () {
        $scope.Status = $scope.Status;
        if ($scope.Status) {
            $scope.Test = "Active";
        }
        else {
            $scope.Test = "InActive";
        }
    }
    $scope.InsertData = function () {
        var Action = document.getElementById("btnSave").getAttribute("value");
        if (Action === "Submit") {
            $scope.Menu = {};
            $scope.Menu.MenuName = $scope.MenuName;
            $scope.Menu.MenuIcon = $scope.MenuIcon;
            $scope.Menu.Status = $scope.Status;
            $http({
                method: "post",
                url: "https://localhost:44369/Account/Insert_Employee",
                datatype: "json",
                data: JSON.stringify($scope.Menu)
            }).then(function (response) {
                toastrMsg(response.data,'success')
                $scope.GetAllData();
                $scope.MenuName = "";
                $scope.MenuIcon = "";
                $scope.Status = true;
            })
        } else {
            $scope.Menu = {};
            $scope.Menu.MenuName = $scope.MenuName;
            $scope.Menu.MenuIcon = $scope.MenuIcon;
            $scope.Menu.Status = $scope.Status;
            $scope.Menu.MenuId = document.getElementById("MenuId_").value;
            $http({
                method: "post",
                url: "https://localhost:44369/Account/Update_Employee",
                datatype: "json",
                data: JSON.stringify($scope.Menu)
            }).then(function (response) {
                toastrMsg(response.data, 'success')
                $scope.GetAllData();
                $scope.MenuName = "";
                $scope.MenuIcon = "";
                $scope.Status = true;
                document.getElementById("btnSave").setAttribute("value", "Submit");
                document.getElementById("btnSave").style.backgroundColor = "cornflowerblue";
                document.getElementById("spn").innerHTML = "Add New Menu";
            })
        }
    }
    $scope.GetAllData = function () {
        $http({
            method: "get",
            url: "https://localhost:44369/Account/Get_AllEmployee"
        }).then(function (response) {           
            $scope.menus = response.data;
        }, function () {
            toastrMsg("Error Occur", 'error')
        })
    };
    $scope.DeleteEmp = function (Emp) {
        $http({
            method: "post",
            url: "https://localhost:44369/Account/Delete_Employee",
            datatype: "json",
            data: JSON.stringify(Emp)
        }).then(function (response) {
            toastrMsg(response.data, 'success')
            $scope.GetAllData();
        })
    };
    $scope.UpdateEmp = function (Emp) {
        document.getElementById("MenuId_").value = Emp.MenuId;
        $scope.MenuName = Emp.MenuName;
        $scope.MenuIcon = Emp.MenuIcon;
        $scope.Status = Emp.Status;
        document.getElementById("btnSave").setAttribute("value", "Update");
        //document.getElementById("btnSave").style.backgroundColor = "Yellow";
        document.getElementById("spn").innerHTML = "Update Menu";
    }
})