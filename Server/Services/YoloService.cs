//using Microsoft.ML;
//using Microsoft.ML.Data;
//using SixLabors.ImageSharp;
//using SixLabors.ImageSharp.Processing;
//using SixLabors.ImageSharp.PixelFormats;

namespace SSTDigitalRD.Server.Services
{
    public class YoloDeteccionResultado
    {
        public bool TienePersonaSinCasco { get; set; }
        public bool TienePersonaSinChaleco { get; set; }
        public int PctCasco { get; set; }
        public int PctChaleco { get; set; }
        public int PctBotas { get; set; }
        public int PersonasDetectadas { get; set; }
        public string Descripcion { get; set; } = "";
    }

    public interface IYoloService
    {
        YoloDeteccionResultado Detectar(byte[] imagenJpeg);
    }

    public class YoloService : IYoloService
    {
        // YOLOv4-Tiny pendiente de integración ONNX
        // Simulación documentada como trabajo futuro (Capítulo 6)
        public YoloDeteccionResultado Detectar(byte[] imagenJpeg)
            => SimularDeteccion();

        private static YoloDeteccionResultado SimularDeteccion()
        {
            var rng = new Random();
            var infraccion = rng.Next(0, 5) == 0;
            return new YoloDeteccionResultado
            {
                TienePersonaSinCasco = infraccion,
                TienePersonaSinChaleco = false,
                PctCasco = infraccion
                    ? rng.Next(60, 80) : rng.Next(88, 100),
                PctChaleco = rng.Next(82, 98),
                PctBotas = rng.Next(75, 95),
                PersonasDetectadas = rng.Next(1, 4),
                Descripcion = infraccion
                    ? "Sin casco detectado · 1 trabajador"
                    : "Todos con EPP · cumplimiento completo"
            };
        }
    }

    //public class YoloService : IYoloService
    //{
    //    private readonly MLContext _mlContext;
    //    private readonly string _modelPath;
    //    private readonly bool _modeloCargado;

    //    // Clases que el modelo YOLOv4 detecta (orden del dataset CHV)
    //    private static readonly string[] Clases = new[]
    //    {
    //        "hardhat", "no_hardhat",
    //        "safety_vest", "no_safety_vest",
    //        "person"
    //    };

    //    public YoloService(IWebHostEnvironment env)
    //    {
    //        _mlContext = new MLContext();
    //        _modelPath = Path.Combine(
    //            env.ContentRootPath, "MLModels", "yolov4.onnx");
    //        _modeloCargado = File.Exists(_modelPath);

    //        if (!_modeloCargado)
    //            Console.WriteLine(
    //                "[YoloService] Modelo ONNX no encontrado " +
    //                $"en: {_modelPath}");
    //    }

    //    public YoloDeteccionResultado Detectar(byte[] imagenJpeg)
    //    {
    //        // Si el modelo no está disponible, devolver
    //        // estimación basada en historial (simulación documentada)
    //        if (!_modeloCargado)
    //            return SimularDeteccion();

    //        try
    //        {
    //            return EjecutarInferencia(imagenJpeg);
    //        }
    //        catch (Exception ex)
    //        {
    //            Console.WriteLine(
    //                $"[YoloService] Error en inferencia: {ex.Message}");
    //            return SimularDeteccion();
    //        }
    //    }

    //    private YoloDeteccionResultado EjecutarInferencia(byte[] imagenJpeg)
    //    {
    //        using var ms = new MemoryStream(imagenJpeg);
    //        //using var img = SixLabors.ImageSharp.Image.Load<SixLabors.ImageSharp.PixelFormats.Rgb24>(ms);
    //        using var img = Image.Load<Rgb24>(ms);

    //        img.Mutate(x => x.Resize(416, 416));

    //        var pixelData = new float[1 * 3 * 416 * 416];
    //        var idx = 0;

    //        for (int c = 0; c < 3; c++)
    //            for (int y = 0; y < 416; y++)
    //                for (int x = 0; x < 416; x++)
    //                {
    //                    var pixel = img[x, y];
    //                    pixelData[idx++] = c == 0 ? pixel.R / 255f
    //                                     : c == 1 ? pixel.G / 255f
    //                                              : pixel.B / 255f;
    //                }

    //        var pipeline = _mlContext.Transforms
    //            .ApplyOnnxModel(
    //                modelFile: _modelPath,
    //                outputColumnNames: new[] { "output" },
    //                inputColumnNames: new[] { "input" });

    //        var inputData = new YoloInput { Image = pixelData };
    //        var dataView = _mlContext.Data
    //            .LoadFromEnumerable(new[] { inputData });

    //        var model = pipeline.Fit(dataView);
    //        var prediction = model.Transform(dataView);

    //        var detecciones = _mlContext.Data
    //            .CreateEnumerable<YoloOutput>(
    //                prediction, reuseRowObject: false)
    //            .FirstOrDefault();

    //        return ProcesarDetecciones(detecciones);
    //    }

    //    private static YoloDeteccionResultado ProcesarDetecciones(
    //        YoloOutput? detecciones)
    //    {
    //        if (detecciones is null)
    //            return SimularDeteccion();

    //        // Las salidas de YOLOv4 incluyen bbox + confianza + clases
    //        // Umbral de confianza para considerar una detección válida
    //        const float umbralConfianza = 0.4f;

    //        int personas = 0;
    //        int conCasco = 0;
    //        int sinCasco = 0;
    //        int conChaleco = 0;
    //        int sinChaleco = 0;

    //        var outputs = detecciones.Output ?? Array.Empty<float>();

    //        // YOLOv4 output: [batch, num_boxes, 5 + num_classes]
    //        // 5 = x, y, w, h, conf
    //        int numClases = Clases.Length;
    //        int entradaPorCaja = 5 + numClases;
    //        int numCajas = outputs.Length / entradaPorCaja;

    //        for (int i = 0; i < numCajas; i++)
    //        {
    //            var offset = i * entradaPorCaja;
    //            var confianza = outputs[offset + 4];
    //            if (confianza < umbralConfianza) continue;

    //            // Encontrar clase con mayor probabilidad
    //            var maxProb = 0f;
    //            var maxClase = 0;
    //            for (int c = 0; c < numClases; c++)
    //            {
    //                var prob = outputs[offset + 5 + c];
    //                if (prob > maxProb)
    //                {
    //                    maxProb = prob;
    //                    maxClase = c;
    //                }
    //            }

    //            switch (Clases[maxClase])
    //            {
    //                case "hardhat": conCasco++; break;
    //                case "no_hardhat": sinCasco++; break;
    //                case "safety_vest": conChaleco++; break;
    //                case "no_safety_vest": sinChaleco++; break;
    //                case "person": personas++; break;
    //            }
    //        }

    //        var totalPersonas = Math.Max(1,
    //            personas + conCasco + sinCasco);

    //        var pctCasco = conCasco + sinCasco > 0
    //            ? (int)(conCasco * 100.0 / (conCasco + sinCasco))
    //            : 100;
    //        var pctChaleco = conChaleco + sinChaleco > 0
    //            ? (int)(conChaleco * 100.0 / (conChaleco + sinChaleco))
    //            : 100;

    //        var tieneInfraccion = sinCasco > 0 || sinChaleco > 0;
    //        var desc = tieneInfraccion
    //            ? $"Sin casco: {sinCasco} · Sin chaleco: {sinChaleco}"
    //            : "Todos con EPP · cumplimiento completo";

    //        return new YoloDeteccionResultado
    //        {
    //            TienePersonaSinCasco = sinCasco > 0,
    //            TienePersonaSinChaleco = sinChaleco > 0,
    //            PctCasco = pctCasco,
    //            PctChaleco = pctChaleco,
    //            PctBotas = 85, // botas no detectadas por este modelo
    //            PersonasDetectadas = totalPersonas,
    //            Descripcion = desc
    //        };
    //    }

    //    private static YoloDeteccionResultado SimularDeteccion()
    //    {
    //        var rng = new Random();
    //        var infraccion = rng.Next(0, 5) == 0;
    //        return new YoloDeteccionResultado
    //        {
    //            TienePersonaSinCasco = infraccion,
    //            TienePersonaSinChaleco = false,
    //            PctCasco = infraccion
    //                ? rng.Next(60, 80) : rng.Next(88, 100),
    //            PctChaleco = rng.Next(82, 98),
    //            PctBotas = rng.Next(75, 95),
    //            PersonasDetectadas = rng.Next(1, 4),
    //            Descripcion = infraccion
    //                ? "Sin casco detectado · 1 trabajador"
    //                : "Todos con EPP · cumplimiento completo"
    //        };
    //    }

    //    // ── Clases de input/output para ML.NET ────────────────
    //    private class YoloInput
    //    {
    //        [ColumnName("input")]
    //        [VectorType(1, 3, 416, 416)]
    //        public float[] Image { get; set; } = Array.Empty<float>();
    //    }

    //    private class YoloOutput
    //    {
    //        [ColumnName("output")]
    //        public float[]? Output { get; set; }
    //    }
    //}
}
