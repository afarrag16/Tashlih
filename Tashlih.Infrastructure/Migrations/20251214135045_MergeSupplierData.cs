using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tashlih.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MergeSupplierData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "logs",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<long>(type: "bigint", nullable: true),
                    action = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    entity_type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    entity_id = table.Column<long>(type: "bigint", nullable: true),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    old_values = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    new_values = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ip_address = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    user_agent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    context = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__logs__3213E83F99C1B5FB", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "part_categories",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    parent_id = table.Column<long>(type: "bigint", nullable: true),
                    name_ar = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    name_en = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    description_ar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    icon = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    image_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    sort_order = table.Column<int>(type: "int", nullable: false),
                    level = table.Column<byte>(type: "tinyint", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__part_cat__3213E83F1131DE08", x => x.id);
                    table.ForeignKey(
                        name: "FK_categories_parent",
                        column: x => x.parent_id,
                        principalTable: "part_categories",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "subscription_plans",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name_ar = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    name_en = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    description_ar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    description_en = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    price = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false, defaultValue: "SAR"),
                    duration_days = table.Column<int>(type: "int", nullable: false, defaultValue: 30),
                    max_parts = table.Column<int>(type: "int", nullable: true),
                    max_images_per_part = table.Column<int>(type: "int", nullable: false, defaultValue: 5),
                    max_shops = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    features = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    sort_order = table.Column<int>(type: "int", nullable: false),
                    is_popular = table.Column<bool>(type: "bit", nullable: false),
                    badge_text = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__subscrip__3213E83F3FCCCA19", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user_otp",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    otp_code = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    purpose = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "login"),
                    expires_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    attempts = table.Column<int>(type: "int", nullable: false),
                    is_used = table.Column<bool>(type: "bit", nullable: false),
                    used_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__user_otp__3213E83F9BF32B49", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    full_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    password_hash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    avatar_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    user_type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "customer"),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "active"),
                    preferred_language = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false, defaultValue: "ar"),
                    notifications_enabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    last_login_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())"),
                    deleted_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__users__3213E83F995BE337", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "vehicle_types",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name_ar = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    name_en = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    icon = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    sort_order = table.Column<int>(type: "int", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__vehicle___3213E83F8A30BF0F", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "attachments",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    attachable_type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    attachable_id = table.Column<long>(type: "bigint", nullable: false),
                    uploaded_by = table.Column<long>(type: "bigint", nullable: true),
                    file_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    file_path = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    file_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    file_type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    file_size = table.Column<int>(type: "int", nullable: true),
                    mime_type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    thumbnail_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    purpose = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    visibility = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "private"),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__attachme__3213E83F8B5EF20A", x => x.id);
                    table.ForeignKey(
                        name: "FK_attachments_uploader",
                        column: x => x.uploaded_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "notifications",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    image_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    data = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    priority = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "normal"),
                    is_read = table.Column<bool>(type: "bit", nullable: false),
                    read_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    is_push_sent = table.Column<bool>(type: "bit", nullable: false),
                    push_sent_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__notifica__3213E83FF5BE0461", x => x.id);
                    table.ForeignKey(
                        name: "FK_notifications_user",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "supplier_profile",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PreferredLanguage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    business_name_ar = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    business_name_en = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    manager_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BusinessType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    city = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    district = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IdFrontUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IdBackUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IdNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CommercialRegisterImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    commercial_register = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CommercialRegisterExpiryDate = table.Column<DateOnly>(type: "date", nullable: true),
                    LicenseImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    license_number = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LicenseExpiryDate = table.Column<DateOnly>(type: "date", nullable: true),
                    TaxCertificateUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tax_number = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    is_verified = table.Column<bool>(type: "bit", nullable: false),
                    verification_status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "not_submitted"),
                    RejectionReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AdminNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VerificationSubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VerificationReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    verified_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    verified_by = table.Column<long>(type: "bigint", nullable: true),
                    rating_average = table.Column<decimal>(type: "decimal(3,2)", nullable: false),
                    rating_count = table.Column<int>(type: "int", nullable: false),
                    total_orders = table.Column<int>(type: "int", nullable: false),
                    completed_orders = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "inactive"),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())"),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    user_id = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__supplier__3213E83F4C7E058B", x => x.id);
                    table.ForeignKey(
                        name: "FK_supplier_user",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_supplier_verified_by",
                        column: x => x.verified_by,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "user_sessions",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    token = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    refresh_token = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    device_type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    device_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    device_info = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ip_address = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    user_agent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    fcm_token = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    expires_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    last_activity_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__user_ses__3213E83F4FF0898D", x => x.id);
                    table.ForeignKey(
                        name: "FK_sessions_user",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "part_category_mapping",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    vehicle_type_id = table.Column<int>(type: "int", nullable: false),
                    category_id = table.Column<long>(type: "bigint", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__part_cat__3213E83F80098EA2", x => x.id);
                    table.ForeignKey(
                        name: "FK_mapping_category",
                        column: x => x.category_id,
                        principalTable: "part_categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_mapping_vehicle",
                        column: x => x.vehicle_type_id,
                        principalTable: "vehicle_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "vehicle_makes",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    vehicle_type_id = table.Column<int>(type: "int", nullable: false),
                    name_ar = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    name_en = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    logo_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    country = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    sort_order = table.Column<int>(type: "int", nullable: false),
                    is_popular = table.Column<bool>(type: "bit", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__vehicle___3213E83FD5DCE6FD", x => x.id);
                    table.ForeignKey(
                        name: "FK_makes_type",
                        column: x => x.vehicle_type_id,
                        principalTable: "vehicle_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "vehicle_subcategories",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    vehicle_type_id = table.Column<int>(type: "int", nullable: false),
                    name_ar = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    name_en = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    icon = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    sort_order = table.Column<int>(type: "int", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__vehicle___3213E83FFAD1E4ED", x => x.id);
                    table.ForeignKey(
                        name: "FK_subcategories_type",
                        column: x => x.vehicle_type_id,
                        principalTable: "vehicle_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "shops",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    supplier_id = table.Column<long>(type: "bigint", nullable: false),
                    name_ar = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    name_en = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    city = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    district = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    street = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    latitude = table.Column<decimal>(type: "decimal(10,8)", nullable: true),
                    longitude = table.Column<decimal>(type: "decimal(11,8)", nullable: true),
                    location_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    whatsapp = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    working_hours = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    is_main = table.Column<bool>(type: "bit", nullable: false),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "active"),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())"),
                    deleted_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__shops__3213E83FF4F788A3", x => x.id);
                    table.ForeignKey(
                        name: "FK_shops_supplier",
                        column: x => x.supplier_id,
                        principalTable: "supplier_profile",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "subscriptions",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    supplier_id = table.Column<long>(type: "bigint", nullable: false),
                    plan_id = table.Column<long>(type: "bigint", nullable: false),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "pending"),
                    starts_at = table.Column<DateOnly>(type: "date", nullable: true),
                    ends_at = table.Column<DateOnly>(type: "date", nullable: true),
                    amount_paid = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    discount_amount = table.Column<decimal>(type: "decimal(10,2)", nullable: true, defaultValue: 0m),
                    payment_method = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    payment_reference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    payment_notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    approved_by = table.Column<long>(type: "bigint", nullable: true),
                    approved_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    rejection_reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    cancelled_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    cancellation_reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    auto_renew = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__subscrip__3213E83F2E0A54E4", x => x.id);
                    table.ForeignKey(
                        name: "FK_subs_approved",
                        column: x => x.approved_by,
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_subs_plan",
                        column: x => x.plan_id,
                        principalTable: "subscription_plans",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_subs_supplier",
                        column: x => x.supplier_id,
                        principalTable: "supplier_profile",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SupplierSessions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SupplierId = table.Column<long>(type: "bigint", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DeviceType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    DeviceName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FcmToken = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastActivityAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupplierSessions_supplier_profile_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "supplier_profile",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "vehicle_models",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    make_id = table.Column<int>(type: "int", nullable: false),
                    name_ar = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    name_en = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    year_from = table.Column<short>(type: "smallint", nullable: true),
                    year_to = table.Column<short>(type: "smallint", nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__vehicle___3213E83F96BE9C1E", x => x.id);
                    table.ForeignKey(
                        name: "FK_models_make",
                        column: x => x.make_id,
                        principalTable: "vehicle_makes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "orders",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    order_number = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    customer_id = table.Column<long>(type: "bigint", nullable: false),
                    shop_id = table.Column<long>(type: "bigint", nullable: false),
                    supplier_id = table.Column<long>(type: "bigint", nullable: false),
                    subtotal = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    discount_amount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    total_amount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false, defaultValue: "SAR"),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "pending"),
                    confirmed_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    processing_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ready_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    completed_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    cancelled_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    cancelled_by = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    cancel_reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    customer_notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    supplier_notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    internal_notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    is_reviewed = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__orders__3213E83FDF81CD1E", x => x.id);
                    table.ForeignKey(
                        name: "FK_orders_customer",
                        column: x => x.customer_id,
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_orders_shop",
                        column: x => x.shop_id,
                        principalTable: "shops",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_orders_supplier",
                        column: x => x.supplier_id,
                        principalTable: "supplier_profile",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "subscription_history",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    subscription_id = table.Column<long>(type: "bigint", nullable: false),
                    supplier_id = table.Column<long>(type: "bigint", nullable: false),
                    action = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    old_status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    new_status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    old_plan_id = table.Column<long>(type: "bigint", nullable: true),
                    new_plan_id = table.Column<long>(type: "bigint", nullable: true),
                    amount = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    performed_by = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__subscrip__3213E83FA3EC7360", x => x.id);
                    table.ForeignKey(
                        name: "FK_history_performer",
                        column: x => x.performed_by,
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_history_sub",
                        column: x => x.subscription_id,
                        principalTable: "subscriptions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_history_supplier",
                        column: x => x.supplier_id,
                        principalTable: "supplier_profile",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "parts",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    shop_id = table.Column<long>(type: "bigint", nullable: false),
                    category_id = table.Column<long>(type: "bigint", nullable: true),
                    name_ar = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    name_en = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    part_number = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    oem_number = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    condition = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "used"),
                    condition_details = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    warranty_days = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    price = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    original_price = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false, defaultValue: "SAR"),
                    quantity = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    views_count = table.Column<int>(type: "int", nullable: false),
                    sales_count = table.Column<int>(type: "int", nullable: false),
                    favorites_count = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "available"),
                    is_featured = table.Column<bool>(type: "bit", nullable: false),
                    featured_until = table.Column<DateOnly>(type: "date", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())"),
                    deleted_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    vehicle_type_id = table.Column<int>(type: "int", nullable: true),
                    vehicle_subcategory_id = table.Column<int>(type: "int", nullable: true),
                    make_id = table.Column<int>(type: "int", nullable: true),
                    model_id = table.Column<int>(type: "int", nullable: true),
                    year_from = table.Column<short>(type: "smallint", nullable: true),
                    year_to = table.Column<short>(type: "smallint", nullable: true),
                    vin_number = table.Column<string>(type: "nvarchar(17)", maxLength: 17, nullable: true),
                    warranty_type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, defaultValue: "none"),
                    delivery_available = table.Column<bool>(type: "bit", nullable: false),
                    delivery_by_shop = table.Column<bool>(type: "bit", nullable: false),
                    delivery_notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    custom_vehicle_type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    custom_subcategory = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    custom_make = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    custom_model = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    custom_category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__parts__3213E83F47584823", x => x.id);
                    table.ForeignKey(
                        name: "FK_parts_category",
                        column: x => x.category_id,
                        principalTable: "part_categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_parts_make",
                        column: x => x.make_id,
                        principalTable: "vehicle_makes",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_parts_model",
                        column: x => x.model_id,
                        principalTable: "vehicle_models",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_parts_shop",
                        column: x => x.shop_id,
                        principalTable: "shops",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_parts_subcategory",
                        column: x => x.vehicle_subcategory_id,
                        principalTable: "vehicle_subcategories",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_parts_vehicle_type",
                        column: x => x.vehicle_type_id,
                        principalTable: "vehicle_types",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "reviews",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    order_id = table.Column<long>(type: "bigint", nullable: false),
                    customer_id = table.Column<long>(type: "bigint", nullable: false),
                    supplier_id = table.Column<long>(type: "bigint", nullable: false),
                    overall_rating = table.Column<byte>(type: "tinyint", nullable: false),
                    quality_rating = table.Column<byte>(type: "tinyint", nullable: true),
                    communication_rating = table.Column<byte>(type: "tinyint", nullable: true),
                    speed_rating = table.Column<byte>(type: "tinyint", nullable: true),
                    price_rating = table.Column<byte>(type: "tinyint", nullable: true),
                    comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    supplier_reply = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    supplier_reply_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    is_verified = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    is_visible = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    is_reported = table.Column<bool>(type: "bit", nullable: false),
                    report_reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    moderated_by = table.Column<long>(type: "bigint", nullable: true),
                    moderated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__reviews__3213E83F1DE91CCF", x => x.id);
                    table.ForeignKey(
                        name: "FK_reviews_customer",
                        column: x => x.customer_id,
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_reviews_moderator",
                        column: x => x.moderated_by,
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_reviews_order",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_reviews_supplier",
                        column: x => x.supplier_id,
                        principalTable: "supplier_profile",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "chat_threads",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    customer_id = table.Column<long>(type: "bigint", nullable: false),
                    supplier_id = table.Column<long>(type: "bigint", nullable: false),
                    order_id = table.Column<long>(type: "bigint", nullable: true),
                    part_id = table.Column<long>(type: "bigint", nullable: true),
                    last_message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    last_message_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    last_message_by = table.Column<long>(type: "bigint", nullable: true),
                    customer_unread_count = table.Column<int>(type: "int", nullable: false),
                    supplier_unread_count = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "active"),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__chat_thr__3213E83FD78A5EF4", x => x.id);
                    table.ForeignKey(
                        name: "FK_threads_customer",
                        column: x => x.customer_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_threads_order",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_threads_part",
                        column: x => x.part_id,
                        principalTable: "parts",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_threads_supplier",
                        column: x => x.supplier_id,
                        principalTable: "supplier_profile",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "order_items",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    order_id = table.Column<long>(type: "bigint", nullable: false),
                    part_id = table.Column<long>(type: "bigint", nullable: true),
                    part_name_snapshot = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    part_number_snapshot = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    condition_snapshot = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    image_url_snapshot = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    quantity = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    unit_price = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    total_price = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    warranty_days_snapshot = table.Column<int>(type: "int", nullable: true),
                    notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__order_it__3213E83FA3D6A6A2", x => x.id);
                    table.ForeignKey(
                        name: "FK_items_order",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_items_part",
                        column: x => x.part_id,
                        principalTable: "parts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "part_images",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    part_id = table.Column<long>(type: "bigint", nullable: false),
                    image_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    thumbnail_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    is_primary = table.Column<bool>(type: "bit", nullable: false),
                    display_order = table.Column<byte>(type: "tinyint", nullable: false),
                    file_size = table.Column<int>(type: "int", nullable: true),
                    mime_type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__part_ima__3213E83F7C51C65E", x => x.id);
                    table.ForeignKey(
                        name: "FK_images_part",
                        column: x => x.part_id,
                        principalTable: "parts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "chat_messages",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    thread_id = table.Column<long>(type: "bigint", nullable: false),
                    sender_id = table.Column<long>(type: "bigint", nullable: false),
                    sender_type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    message_type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "text"),
                    content = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    metadata = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    is_read = table.Column<bool>(type: "bit", nullable: false),
                    read_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    is_deleted = table.Column<bool>(type: "bit", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__chat_mes__3213E83F0E3694C5", x => x.id);
                    table.ForeignKey(
                        name: "FK_messages_sender",
                        column: x => x.sender_id,
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_messages_thread",
                        column: x => x.thread_id,
                        principalTable: "chat_threads",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "chat_attachments",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    message_id = table.Column<long>(type: "bigint", nullable: false),
                    file_type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    file_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    file_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    file_size = table.Column<int>(type: "int", nullable: true),
                    mime_type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    thumbnail_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    width = table.Column<int>(type: "int", nullable: true),
                    height = table.Column<int>(type: "int", nullable: true),
                    duration = table.Column<int>(type: "int", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__chat_att__3213E83F742633AB", x => x.id);
                    table.ForeignKey(
                        name: "FK_chat_attach_msg",
                        column: x => x.message_id,
                        principalTable: "chat_messages",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "idx_attachments_type",
                table: "attachments",
                columns: new[] { "attachable_type", "attachable_id" });

            migrationBuilder.CreateIndex(
                name: "IX_attachments_uploaded_by",
                table: "attachments",
                column: "uploaded_by");

            migrationBuilder.CreateIndex(
                name: "idx_chat_attach_msg",
                table: "chat_attachments",
                column: "message_id");

            migrationBuilder.CreateIndex(
                name: "idx_messages_thread",
                table: "chat_messages",
                column: "thread_id");

            migrationBuilder.CreateIndex(
                name: "IX_chat_messages_sender_id",
                table: "chat_messages",
                column: "sender_id");

            migrationBuilder.CreateIndex(
                name: "idx_threads_customer",
                table: "chat_threads",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "idx_threads_supplier",
                table: "chat_threads",
                column: "supplier_id");

            migrationBuilder.CreateIndex(
                name: "IX_chat_threads_order_id",
                table: "chat_threads",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "IX_chat_threads_part_id",
                table: "chat_threads",
                column: "part_id");

            migrationBuilder.CreateIndex(
                name: "idx_logs_action",
                table: "logs",
                column: "action");

            migrationBuilder.CreateIndex(
                name: "idx_logs_created",
                table: "logs",
                column: "created_at",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "idx_logs_user",
                table: "logs",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "idx_notifications_read",
                table: "notifications",
                columns: new[] { "user_id", "is_read" });

            migrationBuilder.CreateIndex(
                name: "idx_notifications_user",
                table: "notifications",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "idx_items_order",
                table: "order_items",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "IX_order_items_part_id",
                table: "order_items",
                column: "part_id");

            migrationBuilder.CreateIndex(
                name: "idx_orders_customer",
                table: "orders",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "idx_orders_status",
                table: "orders",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "idx_orders_supplier",
                table: "orders",
                column: "supplier_id");

            migrationBuilder.CreateIndex(
                name: "IX_orders_shop_id",
                table: "orders",
                column: "shop_id");

            migrationBuilder.CreateIndex(
                name: "UQ_orders_number",
                table: "orders",
                column: "order_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_categories_active",
                table: "part_categories",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "idx_categories_parent",
                table: "part_categories",
                column: "parent_id");

            migrationBuilder.CreateIndex(
                name: "IX_part_category_mapping_category_id",
                table: "part_category_mapping",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "UQ_mapping",
                table: "part_category_mapping",
                columns: new[] { "vehicle_type_id", "category_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_images_part",
                table: "part_images",
                column: "part_id");

            migrationBuilder.CreateIndex(
                name: "idx_parts_make",
                table: "parts",
                column: "make_id");

            migrationBuilder.CreateIndex(
                name: "idx_parts_model",
                table: "parts",
                column: "model_id");

            migrationBuilder.CreateIndex(
                name: "idx_parts_price",
                table: "parts",
                column: "price");

            migrationBuilder.CreateIndex(
                name: "idx_parts_shop",
                table: "parts",
                column: "shop_id");

            migrationBuilder.CreateIndex(
                name: "idx_parts_status",
                table: "parts",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "idx_parts_vehicle_type",
                table: "parts",
                column: "vehicle_type_id");

            migrationBuilder.CreateIndex(
                name: "idx_parts_year",
                table: "parts",
                columns: new[] { "year_from", "year_to" });

            migrationBuilder.CreateIndex(
                name: "IX_parts_category_id",
                table: "parts",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_parts_vehicle_subcategory_id",
                table: "parts",
                column: "vehicle_subcategory_id");

            migrationBuilder.CreateIndex(
                name: "idx_reviews_supplier",
                table: "reviews",
                column: "supplier_id");

            migrationBuilder.CreateIndex(
                name: "idx_reviews_visible",
                table: "reviews",
                column: "is_visible");

            migrationBuilder.CreateIndex(
                name: "IX_reviews_customer_id",
                table: "reviews",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "IX_reviews_moderated_by",
                table: "reviews",
                column: "moderated_by");

            migrationBuilder.CreateIndex(
                name: "UQ_reviews_order",
                table: "reviews",
                column: "order_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_shops_city",
                table: "shops",
                column: "city");

            migrationBuilder.CreateIndex(
                name: "idx_shops_status",
                table: "shops",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "idx_shops_supplier",
                table: "shops",
                column: "supplier_id");

            migrationBuilder.CreateIndex(
                name: "idx_history_sub",
                table: "subscription_history",
                column: "subscription_id");

            migrationBuilder.CreateIndex(
                name: "IX_subscription_history_performed_by",
                table: "subscription_history",
                column: "performed_by");

            migrationBuilder.CreateIndex(
                name: "IX_subscription_history_supplier_id",
                table: "subscription_history",
                column: "supplier_id");

            migrationBuilder.CreateIndex(
                name: "idx_plans_active",
                table: "subscription_plans",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "idx_subs_status",
                table: "subscriptions",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "idx_subs_supplier",
                table: "subscriptions",
                column: "supplier_id");

            migrationBuilder.CreateIndex(
                name: "IX_subscriptions_approved_by",
                table: "subscriptions",
                column: "approved_by");

            migrationBuilder.CreateIndex(
                name: "IX_subscriptions_plan_id",
                table: "subscriptions",
                column: "plan_id");

            migrationBuilder.CreateIndex(
                name: "idx_supplier_city",
                table: "supplier_profile",
                column: "city");

            migrationBuilder.CreateIndex(
                name: "idx_supplier_status",
                table: "supplier_profile",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "idx_supplier_user",
                table: "supplier_profile",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_supplier_profile_verified_by",
                table: "supplier_profile",
                column: "verified_by");

            migrationBuilder.CreateIndex(
                name: "UQ_supplier_cr",
                table: "supplier_profile",
                column: "commercial_register",
                unique: true,
                filter: "[commercial_register] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UQ_supplier_user",
                table: "supplier_profile",
                column: "user_id",
                unique: true,
                filter: "[user_id] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierSessions_SupplierId",
                table: "SupplierSessions",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierSessions_Token",
                table: "SupplierSessions",
                column: "Token");

            migrationBuilder.CreateIndex(
                name: "idx_otp_expires",
                table: "user_otp",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "idx_otp_phone",
                table: "user_otp",
                column: "phone");

            migrationBuilder.CreateIndex(
                name: "idx_sessions_active",
                table: "user_sessions",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "idx_sessions_user",
                table: "user_sessions",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "idx_users_phone",
                table: "users",
                column: "phone");

            migrationBuilder.CreateIndex(
                name: "idx_users_status",
                table: "users",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "idx_users_type",
                table: "users",
                column: "user_type");

            migrationBuilder.CreateIndex(
                name: "UQ_users_email",
                table: "users",
                column: "email",
                unique: true,
                filter: "[email] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UQ_users_phone",
                table: "users",
                column: "phone",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_makes_popular",
                table: "vehicle_makes",
                column: "is_popular");

            migrationBuilder.CreateIndex(
                name: "idx_makes_type",
                table: "vehicle_makes",
                column: "vehicle_type_id");

            migrationBuilder.CreateIndex(
                name: "idx_models_make",
                table: "vehicle_models",
                column: "make_id");

            migrationBuilder.CreateIndex(
                name: "idx_subcategories_type",
                table: "vehicle_subcategories",
                column: "vehicle_type_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "attachments");

            migrationBuilder.DropTable(
                name: "chat_attachments");

            migrationBuilder.DropTable(
                name: "logs");

            migrationBuilder.DropTable(
                name: "notifications");

            migrationBuilder.DropTable(
                name: "order_items");

            migrationBuilder.DropTable(
                name: "part_category_mapping");

            migrationBuilder.DropTable(
                name: "part_images");

            migrationBuilder.DropTable(
                name: "reviews");

            migrationBuilder.DropTable(
                name: "subscription_history");

            migrationBuilder.DropTable(
                name: "SupplierSessions");

            migrationBuilder.DropTable(
                name: "user_otp");

            migrationBuilder.DropTable(
                name: "user_sessions");

            migrationBuilder.DropTable(
                name: "chat_messages");

            migrationBuilder.DropTable(
                name: "subscriptions");

            migrationBuilder.DropTable(
                name: "chat_threads");

            migrationBuilder.DropTable(
                name: "subscription_plans");

            migrationBuilder.DropTable(
                name: "orders");

            migrationBuilder.DropTable(
                name: "parts");

            migrationBuilder.DropTable(
                name: "part_categories");

            migrationBuilder.DropTable(
                name: "vehicle_models");

            migrationBuilder.DropTable(
                name: "shops");

            migrationBuilder.DropTable(
                name: "vehicle_subcategories");

            migrationBuilder.DropTable(
                name: "vehicle_makes");

            migrationBuilder.DropTable(
                name: "supplier_profile");

            migrationBuilder.DropTable(
                name: "vehicle_types");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
