CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;

CREATE TABLE misiones (
    id uuid NOT NULL,
    nombre character varying(200) NOT NULL,
    estado character varying(50) NOT NULL,
    CONSTRAINT "PK_misiones" PRIMARY KEY (id)
);

CREATE TABLE sesiones (
    id uuid NOT NULL,
    tipo_sesion character varying(50) NOT NULL,
    operador_id uuid NOT NULL,
    estado character varying(50) NOT NULL,
    iniciada_en timestamp with time zone NOT NULL,
    finalizada_en timestamp with time zone,
    CONSTRAINT "PK_sesiones" PRIMARY KEY (id)
);

CREATE TABLE etapas (
    id uuid NOT NULL,
    mision_id uuid NOT NULL,
    orden integer NOT NULL,
    descripcion character varying(500) NOT NULL,
    codigo_qr_solucion character varying(120) NOT NULL,
    CONSTRAINT "PK_etapas" PRIMARY KEY (id),
    CONSTRAINT "FK_etapas_misiones_mision_id" FOREIGN KEY (mision_id) REFERENCES misiones (id) ON DELETE CASCADE
);

CREATE TABLE equipos_sesion (
    id uuid NOT NULL,
    sesion_id uuid NOT NULL,
    nombre character varying(120) NOT NULL,
    codigo_acceso character varying(32) NOT NULL,
    puntaje_total integer NOT NULL,
    CONSTRAINT "PK_equipos_sesion" PRIMARY KEY (id),
    CONSTRAINT "FK_equipos_sesion_sesiones_sesion_id" FOREIGN KEY (sesion_id) REFERENCES sesiones (id) ON DELETE CASCADE
);

CREATE TABLE eventos_sesion (
    id uuid NOT NULL,
    sesion_id uuid NOT NULL,
    tipo character varying(100) NOT NULL,
    payload_json text NOT NULL,
    ocurrido_en timestamp with time zone NOT NULL,
    CONSTRAINT "PK_eventos_sesion" PRIMARY KEY (id),
    CONSTRAINT "FK_eventos_sesion_sesiones_sesion_id" FOREIGN KEY (sesion_id) REFERENCES sesiones (id) ON DELETE CASCADE
);

CREATE TABLE evidencias (
    id uuid NOT NULL,
    sesion_id uuid NOT NULL,
    equipo_id uuid NOT NULL,
    etapa_id uuid NOT NULL,
    codigo_qr_leido character varying(120) NOT NULL,
    timestamp_servidor timestamp with time zone NOT NULL,
    resultado character varying(50) NOT NULL,
    CONSTRAINT "PK_evidencias" PRIMARY KEY (id),
    CONSTRAINT "FK_evidencias_sesiones_sesion_id" FOREIGN KEY (sesion_id) REFERENCES sesiones (id) ON DELETE CASCADE
);

CREATE TABLE pistas (
    id uuid NOT NULL,
    etapa_id uuid NOT NULL,
    contenido character varying(1000) NOT NULL,
    tipo_liberacion character varying(50) NOT NULL,
    segundos_liberacion integer,
    CONSTRAINT "PK_pistas" PRIMARY KEY (id),
    CONSTRAINT "FK_pistas_etapas_etapa_id" FOREIGN KEY (etapa_id) REFERENCES etapas (id) ON DELETE CASCADE
);

CREATE UNIQUE INDEX "IX_equipos_sesion_sesion_id_nombre" ON equipos_sesion (sesion_id, nombre);

CREATE UNIQUE INDEX "IX_etapas_mision_id_orden" ON etapas (mision_id, orden);

CREATE INDEX "IX_eventos_sesion_sesion_id_ocurrido_en" ON eventos_sesion (sesion_id, ocurrido_en);

CREATE INDEX "IX_evidencias_sesion_id" ON evidencias (sesion_id);

CREATE INDEX "IX_pistas_etapa_id" ON pistas (etapa_id);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260527201758_InitialCreate', '8.0.11');

COMMIT;

