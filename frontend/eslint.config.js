// @ts-check
const eslint = require('@eslint/js');
const { defineConfig } = require('eslint/config');
const tseslint = require('typescript-eslint');
const angular = require('angular-eslint');

// Reglas de dependencia entre capas (docs/frontend/PLAN.md, ARQF1 a ARQF3). Las imports
// relativas empiezan con «.»: así 'primeng/api' no se confunde con la carpeta api/.
const HTTP_CLIENT = {
  name: '@angular/common/http',
  message: 'ARQF2: HttpClient sólo se usa en api/ (y se registra en app.config.ts).',
};
const CAPAS_INTERNAS = {
  regex: '^\\.{1,2}/(.*/)?(api|environments)(/|$)',
  message: 'ARQF3: sólo app.config.ts importa api/ y environments/; las vistas usan servicios/.',
};

module.exports = defineConfig([
  {
    files: ['**/*.ts'],
    extends: [
      eslint.configs.recommended,
      tseslint.configs.recommended,
      tseslint.configs.stylistic,
      angular.configs.tsRecommended,
    ],
    processor: angular.processInlineTemplates,
    rules: {
      '@angular-eslint/directive-selector': [
        'error',
        {
          type: 'attribute',
          prefix: 'rrhh',
          style: 'camelCase',
        },
      ],
      '@angular-eslint/component-selector': [
        'error',
        {
          type: 'element',
          prefix: 'rrhh',
          style: 'kebab-case',
        },
      ],
    },
  },
  {
    files: ['**/*.html'],
    extends: [angular.configs.templateRecommended, angular.configs.templateAccessibility],
    rules: {},
  },
  {
    files: ['src/**/*.ts'],
    rules: {
      'no-restricted-globals': [
        'error',
        { name: 'fetch', message: 'ARQF2: las llamadas a la API van por HttpClient, en api/.' },
      ],
    },
  },
  {
    // ARQF1: contratos/ no depende de ninguna otra carpeta; de bibliotecas, sólo tipos
    files: ['src/app/contratos/**/*.ts'],
    rules: {
      '@typescript-eslint/no-restricted-imports': [
        'error',
        {
          patterns: [
            {
              regex: '^\\.\\./',
              message: 'ARQF1: contratos/ no importa otras carpetas del proyecto.',
            },
            {
              regex: '^(?!\\.|rxjs$|@angular/core$|@angular/forms$)',
              message:
                'ARQF1: contratos/ sólo importa tipos de rxjs, @angular/core y @angular/forms.',
            },
            {
              regex: '^(rxjs|@angular/core|@angular/forms)$',
              allowTypeImports: true,
              message:
                'ARQF1: de rxjs, @angular/core y @angular/forms, contratos/ sólo importa tipos (import type).',
            },
          ],
        },
      ],
    },
  },
  {
    // ARQF2: api/ sólo depende de contratos/
    files: ['src/app/api/**/*.ts'],
    rules: {
      '@typescript-eslint/no-restricted-imports': [
        'error',
        {
          patterns: [
            {
              regex: '^\\.\\./(?!contratos/)',
              message: 'ARQF2: api/ sólo importa contratos/.',
            },
          ],
        },
      ],
    },
  },
  {
    // ARQF2 y ARQF3: las capas de arriba no ven api/, environments/ ni HttpClient
    files: ['src/app/servicios/**/*.ts', 'src/app/componentes/**/*.ts', 'src/app/vistas/**/*.ts'],
    rules: {
      '@typescript-eslint/no-restricted-imports': [
        'error',
        { paths: [HTTP_CLIENT], patterns: [CAPAS_INTERNAS] },
      ],
    },
  },
]);
