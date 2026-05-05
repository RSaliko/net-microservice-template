using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderService.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateOrderModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ShippingAddress_ZipCode",
                table: "Orders",
                newName: "Address_ZipCode");

            migrationBuilder.RenameColumn(
                name: "ShippingAddress_Street",
                table: "Orders",
                newName: "Address_Street");

            migrationBuilder.RenameColumn(
                name: "ShippingAddress_State",
                table: "Orders",
                newName: "Address_State");

            migrationBuilder.RenameColumn(
                name: "ShippingAddress_Country",
                table: "Orders",
                newName: "Address_Country");

            migrationBuilder.RenameColumn(
                name: "ShippingAddress_City",
                table: "Orders",
                newName: "Address_City");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Orders",
                newName: "Note");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Orders",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "Orders",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ReceiverName",
                table: "Orders",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ReceiverName",
                table: "Orders");

            migrationBuilder.RenameColumn(
                name: "Address_ZipCode",
                table: "Orders",
                newName: "ShippingAddress_ZipCode");

            migrationBuilder.RenameColumn(
                name: "Address_Street",
                table: "Orders",
                newName: "ShippingAddress_Street");

            migrationBuilder.RenameColumn(
                name: "Address_State",
                table: "Orders",
                newName: "ShippingAddress_State");

            migrationBuilder.RenameColumn(
                name: "Address_Country",
                table: "Orders",
                newName: "ShippingAddress_Country");

            migrationBuilder.RenameColumn(
                name: "Address_City",
                table: "Orders",
                newName: "ShippingAddress_City");

            migrationBuilder.RenameColumn(
                name: "Note",
                table: "Orders",
                newName: "Description");
        }
    }
}
