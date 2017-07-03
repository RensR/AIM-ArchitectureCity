using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Framework.Data;

namespace Framework.Migrations
{
    [DbContext(typeof(PluginContext))]
    [Migration("20161205075102_RMRMigration")]
    partial class RMRMigration
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasAnnotation("ProductVersion", "1.1.0-rtm-22752");

            modelBuilder.Entity("Framework.Models.Event", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("DateTime");

                    b.Property<string>("Name")
                        .IsRequired();

                    b.Property<string>("Origin");

                    b.Property<string>("Thread");

                    b.HasKey("ID");

                    b.ToTable("Event");
                });

            modelBuilder.Entity("Framework.Models.PluginDescription", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Author")
                        .IsRequired();

                    b.Property<string>("Description")
                        .IsRequired();

                    b.Property<string>("FilePath")
                        .IsRequired();

                    b.Property<string>("Name")
                        .IsRequired();

                    b.Property<int>("Type");

                    b.Property<string>("Version")
                        .IsRequired();

                    b.HasKey("ID");

                    b.ToTable("PluginDescription");
                });

            modelBuilder.Entity("Framework.Models.RMRFileTypeModel", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.HasKey("ID");

                    b.ToTable("RMRFileTypeModel");
                });
        }
    }
}
