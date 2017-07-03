using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Framework.Data;

namespace Framework.Migrations
{
    [DbContext(typeof(PluginContext))]
    partial class PluginContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasAnnotation("ProductVersion", "1.1.0-rtm-22752");

            modelBuilder.Entity("Framework.Models.Plugin.Event", b =>
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

            modelBuilder.Entity("Framework.Models.Plugin.PluginDescription", b =>
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

            modelBuilder.Entity("Framework.Models.Plugin.RMRFileTypeModel", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.HasKey("ID");

                    b.ToTable("RMRFileTypeModel");
                });
        }
    }
}
