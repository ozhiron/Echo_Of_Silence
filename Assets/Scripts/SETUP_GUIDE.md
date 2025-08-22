# Руководство по настройке системы локатора / Locator System Setup Guide

## Обзор системы / System Overview

Эта система реализует механику заброса локатора в пруд для сканирования рыб, как описано в Linear issue KOR-13.

This system implements the locator casting mechanics for fish scanning in ponds, as described in Linear issue KOR-13.

## Компоненты системы / System Components

### 1. ResearchChargeSystem (Система зарядов исследования)
- Управляет доступными зарядами для забросов
- Отслеживает лимиты по локациям
- Singleton для глобального доступа

### 2. Locator (Локатор)
- Представляет поплавок/сканер
- Анимация заброса и idle состояние
- Эффекты попадания в воду

### 3. PondInteractionSystem (Система взаимодействия с прудом)
- Обрабатывает клики мыши
- Проверяет валидность позиции заброса
- Создаёт и управляет локаторами

### 4. GameManager (Менеджер игры)
- Координирует все системы
- Обрабатывает события
- Инициализация игры

### 5. ResearchChargeUI (UI зарядов)
- Отображает текущие заряды
- Показывает уведомления
- Визуальная обратная связь

## Настройка в Unity / Unity Setup

### Шаг 1: Создание префаба локатора / Step 1: Create Locator Prefab

1. Создайте пустой GameObject и назовите его "LocatorPrefab"
2. Добавьте компонент SpriteRenderer
3. Добавьте скрипт Locator
4. Настройте параметры анимации в инспекторе
5. Сохраните как префаб

### Шаг 2: Настройка пруда / Step 2: Setup Pond

1. Создайте GameObject для пруда
2. Добавьте Collider2D (например, PolygonCollider2D)
3. Установите соответствующий Layer для пруда
4. Добавьте компонент PondInteractionSystem
5. Настройте параметры в инспекторе:
   - locationId (уникальный ID локации)
   - pondLayerMask (слой пруда)
   - locatorPrefab (ссылка на префаб локатора)
   - playerPosition (позиция игрока)
   - maxCastDistance и minCastDistance

### Шаг 3: Настройка GameManager / Step 3: Setup GameManager

1. Создайте пустой GameObject и назовите его "GameManager"
2. Добавьте скрипт GameManager
3. Создайте ещё один GameObject для "ResearchChargeSystem"
4. Добавьте скрипт ResearchChargeSystem
5. В GameManager назначьте ссылки:
   - researchChargeSystem
   - pondSystems (массив всех PondInteractionSystem)
   - researchChargeUI

### Шаг 4: Настройка UI / Step 4: Setup UI

1. Создайте Canvas для UI
2. Добавьте элементы UI:
   - TextMeshPro для отображения зарядов
   - Slider для визуального индикатора
   - Image[] для иконок зарядов
   - Panel для уведомлений
3. Добавьте скрипт ResearchChargeUI
4. Назначьте ссылки на UI элементы

## Настройка слоёв / Layer Setup

1. Создайте новый слой "Pond" в Layer Manager
2. Назначьте этот слой всем объектам пруда
3. В PondInteractionSystem установите pondLayerMask на слой "Pond"

## Параметры для настройки / Configuration Parameters

### ResearchChargeSystem
- `maxCharges`: Максимальное количество зарядов
- `currentCharges`: Текущие заряды (устанавливается автоматически)
- Location limits настраиваются в коде в методе InitializeLocationLimits()

### PondInteractionSystem
- `locationId`: Уникальный ID локации (например, "Pond_Main")
- `pondLayerMask`: Слой для определения области пруда
- `maxCastDistance`: Максимальная дистанция заброса
- `minCastDistance`: Минимальная дистанция заброса

### Locator
- `idleAnimationSpeed`: Скорость idle анимации
- `bobAmount`: Амплитуда покачивания
- `throwDuration`: Длительность анимации заброса
- `throwHeight`: Высота параболы заброса

## Использование / Usage

1. Игрок кликает ЛКМ по пруду
2. Система проверяет наличие зарядов и лимиты локации
3. При успешной проверке создаётся локатор
4. Проигрывается анимация заброса
5. Локатор переходит в idle состояние на поверхности воды
6. UI обновляется, показывая новое количество зарядов

## События / Events

Система использует события для связи между компонентами:
- `OnChargesChanged`: Изменение количества зарядов
- `OnLocationLimitReached`: Достижение лимита локации
- `OnLocatorCast`: Успешный заброс локатора
- `OnLocatorLanded`: Локатор приземлился
- `OnCastFailed`: Неудачный заброс

## Расширение системы / System Extensions

Система спроектирована для лёгкого расширения:
- Добавление новых типов локаторов
- Различные эффекты для разных водоёмов
- Система улучшений зарядов
- Интеграция с системой рыбалки/сканирования