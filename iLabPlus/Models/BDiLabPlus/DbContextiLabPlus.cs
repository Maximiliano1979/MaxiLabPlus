using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

namespace iLabPlus.Models.BDiLabPlus
{
    public partial class DbContextiLabPlus : DbContext
    {
        public DbContextiLabPlus()
        {            
        }

        public DbContextiLabPlus(DbContextOptions<DbContextiLabPlus> options)
            : base(options)
        {
        }

        public virtual DbSet<Empresas>              Empresas                { get; set; }
        public virtual DbSet<EmpresasConfig>        EmpresasConfig          { get; set; }
        public virtual DbSet<EmpresasCertificados>  EmpresasCertificados    { get; set; }

        public virtual DbSet<Clientes>          Clientes            { get; set; }
        public virtual DbSet<Usuarios>          Usuarios            { get; set; }
        public virtual DbSet<UsuariosGridsCfg>  UsuariosGridsCfg    { get; set; }
        public virtual DbSet<ValSys>            ValSys              { get; set; }
        public virtual DbSet<Articulos>         Articulos           { get; set; }
        public virtual DbSet<ArticulosMO>       ArticulosMO         { get; set; }
        public virtual DbSet<ArticulosCOMP>     ArticulosCOMP       { get; set; }
        public virtual DbSet<Vendedores>        Vendedores          { get; set; }
        public virtual DbSet<OperariosEXT>      OperariosEXT        { get; set; }
        public virtual DbSet<TarifasVenta>      TarifasVenta        { get; set; }
        public virtual DbSet<Proveedores>       Proveedores         { get; set; }
        public virtual DbSet<Divisas>           Divisas             { get; set; }
        public virtual DbSet<DivisasDet>        DivisasDet          { get; set; }
        public virtual DbSet<Stocks>            Stocks              { get; set; }
        public virtual DbSet<StocksALM>         StocksALM           { get; set; }
        public virtual DbSet<ContaBancos>       ContaBancos         { get; set; }
        public virtual DbSet<ContaRemesas>      ContaRemesas        { get; set; }
        public virtual DbSet<ContaRemesasLIN>   ContaRemesasLIN     { get; set; }

        public virtual DbSet<ArticulosIMG>          ArticulosIMG            { get; set; }
        public virtual DbSet<Fixing>                Fixing                  { get; set; }

        public virtual DbSet<PedidosCAB>            PedidosCAB              { get; set; }
        public virtual DbSet<PedidosLIN>            PedidosLIN              { get; set; }

        public virtual DbSet<PedidosLINCOMP>        PedidosLINCOMP          { get; set; }
        public virtual DbSet<MrpPlanesCAB>          MrpPlanesCAB            { get; set; }
        public virtual DbSet<MrpPlanesLIN>          MrpPlanesLIN            { get; set; }
        public virtual DbSet<MrpPlanesLINCOMP>      MrpPlanesLINCOMP        { get; set; }
        public virtual DbSet<MrpPlanesLINMO>        MrpPlanesLINMO          { get; set; }

        public virtual DbSet<Logs_Versiones>        Logs_Versiones          { get; set; }
        public virtual DbSet<Logs_Accesos>          Logs_Accesos            { get; set; }


        public virtual DbSet<FacturasCAB>           FacturasCAB             { get; set; }
        public virtual DbSet<FacturasLIN>           FacturasLIN             { get; set; }
        public virtual DbSet<ProvFacturasCAB>       ProvFacturasCAB         { get; set; }
        public virtual DbSet<ProvFacturasLIN>       ProvFacturasLIN         { get; set; }

        public virtual DbSet<Calendario>                Calendario              { get; set; }
        public virtual DbSet<CalendarioMultiUsuarios>   CalendarioMultiUsuarios { get; set; }

        
        public virtual DbSet<CorreosSalientes>          CorreosSalientes        { get; set; }
        public virtual DbSet<CorreosSalientesAdj>       CorreosSalientesAdj     { get; set; }

        public virtual DbSet<Empleados>                 Empleados               { get; set; }

        public virtual DbSet<ControlHorario>            ControlHorario          { get; set; }
        public virtual DbSet<LeyAntiFraude112021>       LeyAntiFraude112021     { get; set; }
        public virtual DbSet<PlantillasEnvios>          PlantillasEnvios        { get; set; }
        public virtual DbSet<HannaAIChat>               HannaAIChat             { get; set; }

        public virtual DbSet<Contactos>                 Contactos               { get; set; }

        public virtual DbSet<Fases>                     Fases                   { get; set; }

        public virtual DbSet<FasesEmpleados>            FasesEmpleados          { get; set; }

        public virtual DbSet<Doctores>                  Doctores                 { get; set; }

        public virtual DbSet<DoctoresClinicas>          DoctoresClinicas                { get; set; }





        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<Doctores>(entity =>
            {
                entity.HasKey(e => new { e.Guid });

            });

            modelBuilder.Entity<DoctoresClinicas>(entity =>
            {
                entity.HasKey(e => new { e.Guid });

            });

            modelBuilder.Entity<FasesEmpleados>(entity =>
            {
                entity.HasKey(e => new { e.Empresa, e.Fase, e.Empleado });
            });

            modelBuilder.Entity<Fases>(entity =>
            {
                entity.HasKey(e => new { e.Guid });

            });

            modelBuilder.Entity<Contactos>(entity =>
            {
                entity.HasKey(e => e.Guid); 
            });

            modelBuilder.Entity<HannaAIChat>(entity =>
            {
                entity.HasKey(e => e.Guid);
            });

            modelBuilder.Entity<PlantillasEnvios>(entity =>
            {
                entity.HasKey(e => e.Guid);
            });

            modelBuilder.Entity<LeyAntiFraude112021>(entity =>
            {
                entity.HasKey(e => e.Guid);
            });


            modelBuilder.Entity<ControlHorario>(entity =>
            {
                entity.HasKey(e => e.Guid);
            });


            modelBuilder.Entity<Empleados>(entity =>
            {
                entity.HasKey(e => e.Guid);
            });


            modelBuilder.Entity<CorreosSalientesAdj>(entity =>
            {
                entity.HasKey(e => e.Guid); 
            });

            modelBuilder.Entity<CorreosSalientes>(entity =>
            {
                entity.HasKey(e => e.Guid);
            });



            modelBuilder.Entity<EmpresasCertificados>(entity =>
            {
                entity.HasKey(e => new { e.Guid });
            });

            modelBuilder.Entity<ProvFacturasCAB>(entity =>
            {
                entity.HasKey(e => new { e.Guid });
            });


            modelBuilder.Entity<ProvFacturasLIN>(entity =>
            {
                entity.HasKey(e => new { e.Guid });
            });

            modelBuilder.Entity<Calendario>(entity =>
            {
                entity.HasKey(e => new { e.Guid });
            });

            modelBuilder.Entity<CalendarioMultiUsuarios>(entity =>
            {
                //entity.HasKey(e => new { e.GuidEvento });
                entity.HasKey(e => new { e.Guid });
            });

            modelBuilder.Entity<FacturasCAB>(entity =>
            {
                entity.HasKey(e => new { e.Guid });
            });

            modelBuilder.Entity<FacturasLIN>(entity =>
            {
                entity.HasKey(e => new { e.Guid });
            });


            modelBuilder.Entity<Logs_Accesos>(entity =>
            {
                entity.HasKey(e => new { e.Guid });
            });
            modelBuilder.Entity<Logs_Versiones>(entity =>
            {
                entity.HasKey(e => new { e.Guid });
            });

            modelBuilder.Entity<MrpPlanesCAB>(entity =>
            {
                entity.HasKey(e => new { e.Guid });
            });

            modelBuilder.Entity<MrpPlanesLIN>(entity =>
            {
                entity.HasKey(e => new { e.Guid });
            });

            modelBuilder.Entity<MrpPlanesLINCOMP>(entity =>
            {
                entity.HasKey(e => new { e.Guid });
            });

            modelBuilder.Entity<MrpPlanesLINMO>(entity =>
            {
                entity.HasKey(e => new { e.Guid });
            });

            modelBuilder.Entity<PedidosCAB>(entity =>
            {
                entity.HasKey(e => new { e.Guid });

                entity.Property(e => e.PedOroFino)
                    .HasColumnName("PedOroFino")
                    .HasColumnType("decimal(15, 5)");

                entity.Property(e => e.PedIVA)
                    .HasColumnName("PedIVA")
                    .HasColumnType("decimal(15, 2)");
                entity.Property(e => e.PedRE)
                    .HasColumnName("PedRE")
                    .HasColumnType("decimal(15, 2)");
                entity.Property(e => e.PedIGIC)
                    .HasColumnName("PedIGIC")
                    .HasColumnType("decimal(15, 2)");
                entity.Property(e => e.PedIRPF)
                    .HasColumnName("PedIRPF")
                    .HasColumnType("decimal(15, 2)");

                entity.Property(e => e.PedDatFPint)
                    .HasColumnName("PedDatFPint")
                    .HasColumnType("decimal(15, 0)");
                entity.Property(e => e.PedDatFPlazo)
                    .HasColumnName("PedDatFPlazo")
                    .HasColumnType("decimal(15, 0)");
                entity.Property(e => e.PedDatFPvto)
                    .HasColumnName("PedDatFPvto")
                    .HasColumnType("decimal(15, 0)");
                entity.Property(e => e.PedDatFPdia1)
                    .HasColumnName("PedDatFPdia1")
                    .HasColumnType("decimal(15, 0)");
                entity.Property(e => e.PedDatFPdia2)
                    .HasColumnName("PedDatFPdia2")
                    .HasColumnType("decimal(15, 0)");
                entity.Property(e => e.PedDatFPdia3)
                    .HasColumnName("PedDatFPdia3")
                    .HasColumnType("decimal(15, 0)");

                entity.Property(e => e.PedDTOCial)
                    .HasColumnName("PedDTOCial")
                    .HasColumnType("decimal(15, 2)");
                entity.Property(e => e.PedDTOPpago)
                    .HasColumnName("PedDTOPpago")
                    .HasColumnType("decimal(15, 2)");
                entity.Property(e => e.PedDTORappel)
                    .HasColumnName("PedDTORappel")
                    .HasColumnType("decimal(15, 2)");

            });

            modelBuilder.Entity<PedidosLIN>(entity =>
            {
                entity.HasKey(e => new { e.Guid });

            });


            modelBuilder.Entity<PedidosLINCOMP>(entity =>
            {
                entity.HasKey(e => new { e.Guid });

            });

            modelBuilder.Entity<Fixing>(entity =>
            {
                entity.HasKey(e => new { e.Guid });

                entity.Property(e => e.K24Milesimas)
                    .HasColumnName("K24Milesimas")
                    .HasColumnType("decimal(15, 3)");
                entity.Property(e => e.K24Ley)
                    .HasColumnName("K24Ley")
                    .HasColumnType("decimal(15, 3)");

                entity.Property(e => e.K19Milesimas)
                    .HasColumnName("K19Milesimas")
                    .HasColumnType("decimal(15, 3)");
                entity.Property(e => e.K19Ley)
                    .HasColumnName("K19Ley")
                    .HasColumnType("decimal(15, 3)");

                entity.Property(e => e.K18Milesimas)
                    .HasColumnName("K18Milesimas")
                    .HasColumnType("decimal(15, 3)");
                entity.Property(e => e.K18Ley)
                    .HasColumnName("K18Ley")
                    .HasColumnType("decimal(15, 3)");

                entity.Property(e => e.K14Milesimas)
                    .HasColumnName("K14Milesimas")
                    .HasColumnType("decimal(15, 3)");
                entity.Property(e => e.K14Ley)
                    .HasColumnName("K14Ley")
                    .HasColumnType("decimal(15, 3)");

                entity.Property(e => e.K10Milesimas)
                    .HasColumnName("K10Milesimas")
                    .HasColumnType("decimal(15, 3)");
                entity.Property(e => e.K10Ley)
                    .HasColumnName("K10Ley")
                    .HasColumnType("decimal(15, 3)");

                entity.Property(e => e.K9Milesimas)
                    .HasColumnName("K9Milesimas")
                    .HasColumnType("decimal(15, 3)");
                entity.Property(e => e.K9Ley)
                    .HasColumnName("K9Ley")
                    .HasColumnType("decimal(15, 3)");

                entity.Property(e => e.K8Milesimas)
                    .HasColumnName("K8Milesimas")
                    .HasColumnType("decimal(15, 3)");
                entity.Property(e => e.K8Ley)
                    .HasColumnName("K8Ley")
                    .HasColumnType("decimal(15, 3)");

            });


            modelBuilder.Entity<ArticulosIMG>(entity =>
            {
                entity.HasKey(e => new { e.Guid });

            });

            modelBuilder.Entity<ContaRemesas>(entity =>
            {
                entity.HasKey(e => new { e.Guid });
            });

            modelBuilder.Entity<ContaRemesasLIN>(entity =>
            {
                entity.HasKey(e => new { e.Guid });
            });


            modelBuilder.Entity<ContaBancos>(entity =>
            {
                entity.HasKey(e => new { e.Guid });
            });

            modelBuilder.Entity<StocksALM>(entity =>
            {
                entity.HasKey(e => new { e.Guid });
            });

            modelBuilder.Entity<Stocks>(entity =>
            {
                entity.HasKey(e => new { e.Guid });

                entity.Property(e => e.StkFisico)
                    .HasColumnName("StkFisico")
                    .HasColumnType("decimal(15, 5)");
                entity.Property(e => e.StkMaximo)
                    .HasColumnName("StkMaximo")
                    .HasColumnType("decimal(15, 5)");
                entity.Property(e => e.StkMinimo)
                    .HasColumnName("StkMinimo")
                    .HasColumnType("decimal(15, 5)");

                entity.Property(e => e.StkFisicoPeso)
                    .HasColumnName("StkFisicoPeso")
                    .HasColumnType("decimal(15, 5)");
                entity.Property(e => e.StkReservado)
                    .HasColumnName("StkReservado")
                    .HasColumnType("decimal(15, 5)");
                entity.Property(e => e.StkIniFIS)
                    .HasColumnName("StkIniFIS")
                    .HasColumnType("decimal(15, 5)");

                entity.Property(e => e.StkIniFISpes)
                    .HasColumnName("StkIniFISpes")
                    .HasColumnType("decimal(15, 5)");
                entity.Property(e => e.StkCiclico)
                    .HasColumnName("StkCiclico")
                    .HasColumnType("decimal(15, 5)");
                entity.Property(e => e.StkCiclicoPeso)
                    .HasColumnName("StkCiclicoPeso")
                    .HasColumnType("decimal(15, 5)");

            });


            modelBuilder.Entity<DivisasDet>(entity =>
            {
                entity.HasKey(e => new { e.Guid });

                entity.Property(e => e.DivCambio)
                    .HasColumnName("DivCambio")
                    .HasColumnType("decimal(15, 5)");
            });

            modelBuilder.Entity<Divisas>(entity =>
            {
                entity.HasKey(e => new { e.Guid });

                entity.Property(e => e.DivCambio)
                    .HasColumnName("DivCambio")
                    .HasColumnType("decimal(15, 5)");
            });

            modelBuilder.Entity<Proveedores>(entity =>
            {
                entity.HasKey(e => new { e.Guid });

            });

            modelBuilder.Entity<TarifasVenta>(entity =>
            {
                entity.HasKey(e => new { e.Guid });

            });

            modelBuilder.Entity<Vendedores>(entity =>
            {
                entity.HasKey(e => new { e.Guid });

            });

            modelBuilder.Entity<OperariosEXT>(entity =>
            {
                entity.HasKey(e => new { e.Guid });

            });


            modelBuilder.Entity<ArticulosCOMP>(entity =>
            {
                entity.HasKey(e => new { e.Guid });
            });


            modelBuilder.Entity<ArticulosMO>(entity =>
            {
                entity.HasKey(e => new { e.Guid });
            });

            modelBuilder.Entity<Articulos>(entity =>
            {
                entity.HasKey(e => new { e.Guid });

                entity.Property(e => e.ArtPrecioCoste)
                    .HasColumnName("ArtPrecioCoste")
                    .HasColumnType("decimal(15, 5)");

                entity.Property(e => e.ArtPrecioCompra)
                    .HasColumnName("ArtPrecioCompra")
                    .HasColumnType("decimal(15, 5)");

                entity.Property(e => e.ArtPrecioPVP)
                    .HasColumnName("ArtPrecioPVP")
                    .HasColumnType("decimal(15, 5)");

                entity.Property(e => e.ArtPeso)
                    .HasColumnName("ArtPeso")
                    .HasColumnType("decimal(15, 5)");



                entity.Property(e => e.CompraMonedaCoti)
                    .HasColumnName("CompraMonedaCoti")
                    .HasColumnType("decimal(15, 5)");
                

            });

            modelBuilder.Entity<ValSys>(entity =>
            {
                entity.HasKey(e => new { e.Guid });

                entity.Property(e => e.Valor4)
                    .HasColumnName("Valor4")
                    .HasColumnType("decimal(15, 5)");

            });

            modelBuilder.Entity<Usuarios>(entity =>
            {
                entity.HasKey(e => new { e.Guid });

            });

            modelBuilder.Entity<UsuariosGridsCfg>(entity =>
            {
                entity.HasKey(e => new { e.Guid });

            });


            modelBuilder.Entity<Clientes>(entity =>
            {
                entity.HasKey(e => new { e.Guid });

            });

            

            modelBuilder.Entity<EmpresasConfig>(entity =>
            {
                entity.HasKey(e => new { e.Guid });

            });

            modelBuilder.Entity<Empresas>(entity =>
            {
                entity.HasKey(e => new { e.Guid });

            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
