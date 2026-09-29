namespace RRHH.ArchitectureTests;

using System.Reflection;
using System.Xml.Linq;

using NetArchTest.Rules;

public class ArquitecturaTests
{
    [Fact]
    public void VerificarDependencias_DomainYContratos_NoDependenDeOtrosProyectos()
    {
        // Arrange
        var raiz = ObtenerRaizRepositorio();
        var domainCsproj = Path.Combine(raiz, "src", "RRHH.Domain", "RRHH.Domain.csproj");
        var contratosCsproj = Path.Combine(raiz, "src", "RRHH.Contratos", "RRHH.Contratos.csproj");

        string[] prohibidosDomain = ["RRHH.Contratos", "RRHH.Application", "RRHH.Infrastructure", "RRHH.Api", "RRHH.Web"];
        string[] prohibidosContratos = ["RRHH.Domain", "RRHH.Application", "RRHH.Infrastructure", "RRHH.Api", "RRHH.Web"];

        var domainAssembly = CargarEnsamblado("RRHH.Domain");
        var contratosAssembly = CargarEnsamblado("RRHH.Contratos");

        // Act
        var refsDomain = ObtenerReferenciasDeProyecto(domainCsproj);
        var refsContratos = ObtenerReferenciasDeProyecto(contratosCsproj);

        var resultadoTiposDomain = Types.InAssembly(domainAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(prohibidosDomain)
            .GetResult();

        var resultadoTiposContratos = Types.InAssembly(contratosAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(prohibidosContratos)
            .GetResult();

        // Assert
        Assert.Empty(refsDomain);
        Assert.Empty(refsContratos);
        Assert.True(resultadoTiposDomain.IsSuccessful,
            $"RRHH.Domain tiene tipos con dependencias prohibidas: {string.Join(", ", resultadoTiposDomain.FailingTypeNames ?? [])}");
        Assert.True(resultadoTiposContratos.IsSuccessful,
            $"RRHH.Contratos tiene tipos con dependencias prohibidas: {string.Join(", ", resultadoTiposContratos.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void VerificarDependencias_Application_NoDependeDeInfraestructuraApiNiWeb()
    {
        // Arrange
        var raiz = ObtenerRaizRepositorio();
        var applicationCsproj = Path.Combine(raiz, "src", "RRHH.Application", "RRHH.Application.csproj");
        string[] prohibidosApplication = ["RRHH.Infrastructure", "RRHH.Api", "RRHH.Web"];
        var applicationAssembly = CargarEnsamblado("RRHH.Application");

        // Act
        var referenciasProyecto = ObtenerReferenciasDeProyecto(applicationCsproj);
        var referenciasProhibidas = referenciasProyecto.Intersect(prohibidosApplication).ToList();

        var resultadoTipos = Types.InAssembly(applicationAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(prohibidosApplication)
            .GetResult();

        // Assert
        Assert.Empty(referenciasProhibidas);
        Assert.True(resultadoTipos.IsSuccessful,
            $"RRHH.Application tiene tipos con dependencias prohibidas: {string.Join(", ", resultadoTipos.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void VerificarDependencias_Web_SoloDependeDeContratos()
    {
        // Arrange
        var raiz = ObtenerRaizRepositorio();
        var webCsproj = Path.Combine(raiz, "src", "RRHH.Web", "RRHH.Web.csproj");
        string[] prohibidosWeb = ["RRHH.Domain", "RRHH.Application", "RRHH.Infrastructure", "RRHH.Api"];
        var webAssembly = CargarEnsamblado("RRHH.Web");

        // Act
        var referenciasProyecto = ObtenerReferenciasDeProyecto(webCsproj);

        var resultadoTipos = Types.InAssembly(webAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(prohibidosWeb)
            .GetResult();

        // Assert
        var referenciaUnica = Assert.Single(referenciasProyecto);
        Assert.Equal("RRHH.Contratos", referenciaUnica);
        Assert.True(resultadoTipos.IsSuccessful,
            $"RRHH.Web tiene tipos con dependencias prohibidas: {string.Join(", ", resultadoTipos.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void VerificarDependencias_ControladoresApi_NoUsanTiposDeDominio()
    {
        // Arrange
        var apiAssembly = CargarEnsamblado("RRHH.Api");
        var controladores = apiAssembly.GetTypes().Where(EsControlador).ToList();
        var violaciones = new List<string>();

        // Act
        foreach (var controlador in controladores)
        {
            var metodos = ObtenerMetodosAccion(controlador);
            foreach (var metodo in metodos)
            {
                if (ContieneTipoDeDominio(metodo.ReturnType))
                {
                    violaciones.Add($"{controlador.FullName}.{metodo.Name} retorna {metodo.ReturnType.Name} con tipos de RRHH.Domain");
                }
            }
        }

        var resultadoTiposControladores = Types.InAssembly(apiAssembly)
            .That()
            .ResideInNamespace("RRHH.Api")
            .And()
            .HaveNameEndingWith("Controller")
            .ShouldNot()
            .HaveDependencyOn("RRHH.Domain")
            .GetResult();

        // Assert
        Assert.Empty(violaciones);
        Assert.True(resultadoTiposControladores.IsSuccessful,
            $"Controladores de API tienen dependencias de RRHH.Domain: {string.Join(", ", resultadoTiposControladores.FailingTypeNames ?? [])}");
    }

    private static string ObtenerRaizRepositorio()
    {
        var directorio = new DirectoryInfo(AppContext.BaseDirectory);
        while (directorio != null &&
               !File.Exists(Path.Combine(directorio.FullName, "RRHH.slnx")))
        {
            directorio = directorio.Parent;
        }

        return directorio?.FullName
            ?? throw new InvalidOperationException("No se encontró la raíz del repositorio.");
    }

    private static IReadOnlyList<string> ObtenerReferenciasDeProyecto(string rutaCsproj)
    {
        if (!File.Exists(rutaCsproj))
        {
            throw new FileNotFoundException($"No se encontró el archivo de proyecto en {rutaCsproj}");
        }

        var doc = XDocument.Load(rutaCsproj);
        return doc.Descendants("ProjectReference")
            .Select(x => (string?)x.Attribute("Include"))
            .Where(include => !string.IsNullOrWhiteSpace(include))
            .Select(include => Path.GetFileNameWithoutExtension(include!.Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar)))
            .ToList();
    }

    private static Assembly CargarEnsamblado(string nombre)
    {
        try
        {
            return Assembly.Load(new AssemblyName(nombre));
        }
        catch
        {
            return Assembly.LoadFrom(Path.Combine(AppContext.BaseDirectory, $"{nombre}.dll"));
        }
    }

    private static bool EsControlador(Type tipo)
    {
        if (!tipo.IsClass || tipo.IsAbstract || !tipo.IsPublic)
        {
            return false;
        }

        if (tipo.Name.EndsWith("Controller", StringComparison.Ordinal))
        {
            return true;
        }

        var baseType = tipo.BaseType;
        while (baseType != null)
        {
            if (baseType.Name is "ControllerBase" or "Controller")
            {
                return true;
            }
            baseType = baseType.BaseType;
        }

        return tipo.GetCustomAttributes(inherit: true)
            .Any(a => a.GetType().Name is "ApiControllerAttribute" or "ControllerAttribute");
    }

    private static IEnumerable<MethodInfo> ObtenerMetodosAccion(Type tipoControlador)
    {
        return tipoControlador.GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.DeclaringType != typeof(object) &&
                        m.DeclaringType?.Name is not ("ControllerBase" or "Controller") &&
                        !m.IsSpecialName &&
                        !m.GetCustomAttributes(inherit: true).Any(a => a.GetType().Name == "NonActionAttribute"));
    }

    private static bool ContieneTipoDeDominio(Type? tipo, HashSet<Type>? visitados = null)
    {
        if (tipo == null || tipo == typeof(void))
        {
            return false;
        }

        visitados ??= new HashSet<Type>();
        if (!visitados.Add(tipo))
        {
            return false;
        }

        if (EsTipoDeDominio(tipo))
        {
            return true;
        }

        if (tipo.IsArray)
        {
            var elemento = tipo.GetElementType();
            if (elemento != null && ContieneTipoDeDominio(elemento, visitados))
            {
                return true;
            }
        }

        if (tipo.IsGenericType)
        {
            foreach (var argumento in tipo.GetGenericArguments())
            {
                if (ContieneTipoDeDominio(argumento, visitados))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool EsTipoDeDominio(Type tipo)
    {
        var nombreNamespace = tipo.Namespace ?? string.Empty;
        var nombreEnsamblado = tipo.Assembly.GetName().Name ?? string.Empty;

        return nombreNamespace.Equals("RRHH.Domain", StringComparison.Ordinal)
            || nombreNamespace.StartsWith("RRHH.Domain.", StringComparison.Ordinal)
            || nombreEnsamblado.Equals("RRHH.Domain", StringComparison.Ordinal);
    }
}
