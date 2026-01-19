# Инструкция по созданию врагов

## Шаг 1: Создать ScriptableObjects для врагов

В Unity создайте ScriptableObject для каждого типа:
1. ПКМ в папке Assets/Scripts → Create → Game → Enemy Data
2. Настройте характеристики согласно типу:

### Common Ant (Базовый муравей)
- **Name**: Common Ant
- **Type**: Common
- **Max Health**: 20
- **Move Speed**: 2
- **Attack Damage**: 5
- **Attack Cooldown**: 1
- **Biomass Drop**: 2-5
- **Can Shoot**: false
- **Tint Color**: White (1, 1, 1, 1)
- **Scale**: 1.0

### Shooting Ant (Стреляющий)
- **Name**: Shooting Ant
- **Type**: Shooting
- **Max Health**: 15
- **Move Speed**: 1.5
- **Attack Damage**: 8
- **Attack Cooldown**: 1.5
- **Biomass Drop**: 3-7
- **Can Shoot**: true ✓
- **Projectile Prefab**: EnemyBullet (создать отдельно)
- **Shoot Cooldown**: 2
- **Shoot Range**: 8
- **Tint Color**: Yellow (1, 1, 0.5, 1)
- **Scale**: 1.0

### Tank Ant (Танк)
- **Name**: Tank Ant
- **Type**: Tank
- **Max Health**: 60
- **Move Speed**: 1.0
- **Attack Damage**: 15
- **Attack Cooldown**: 2
- **Biomass Drop**: 10-15
- **Can Shoot**: false
- **Tint Color**: Red (1, 0.3, 0.3, 1)
- **Scale**: 1.3

### Soldier Ant (Солдат)
- **Name**: Soldier Ant
- **Type**: Soldier
- **Max Health**: 35
- **Move Speed**: 3.0
- **Attack Damage**: 12
- **Attack Cooldown**: 0.8
- **Biomass Drop**: 5-10
- **Can Shoot**: false
- **Tint Color**: Orange (1, 0.5, 0, 1)
- **Scale**: 1.1

### Boss Ant (Босс)
- **Name**: Boss Ant
- **Type**: Boss
- **Max Health**: 200
- **Move Speed**: 1.5
- **Attack Damage**: 25
- **Attack Cooldown**: 1.5
- **Biomass Drop**: 50-100
- **Can Shoot**: true ✓
- **Projectile Prefab**: EnemyBullet
- **Shoot Cooldown**: 1.5
- **Shoot Range**: 10
- **Is Boss**: true ✓
- **Boss Phase**: 2
- **Tint Color**: Purple (0.5, 0, 1, 1)
- **Scale**: 2.0

### Goblin (Быстрый гоблин)
- **Name**: Goblin
- **Type**: Goblin
- **Max Health**: 10
- **Move Speed**: 4.0
- **Attack Damage**: 3
- **Attack Cooldown**: 0.5
- **Biomass Drop**: 1-3
- **Can Shoot**: false
- **Tint Color**: Green (0.3, 1, 0.3, 1)
- **Scale**: 0.7

---

## Шаг 2: Создать префабы врагов

Для каждого типа:

1. **Создайте GameObject** в сцене
2. **Добавьте компоненты**:
   - SpriteRenderer (назначьте спрайт муравья)
   - Animator (используйте AntAnimatorController)
   - Rigidbody2D (Body Type = Kinematic, Gravity Scale = 0)
   - CircleCollider2D (Is Trigger = true, Radius = 0.3)
   - Health (скрипт)
   - EnemyAI (скрипт)
3. **Настройте EnemyAI**:
   - В поле Enemy Data перетащите созданный ScriptableObject
4. **Тег**: Установите тег "Enemy"
5. **Сохраните как префаб** в Assets/Prefabs/Enemies/

---

## Шаг 3: Создать снаряд для стреляющих врагов

1. **GameObject** → Create Empty → EnemyBullet
2. **Добавьте**:
   - SpriteRenderer (небольшой красный круг)
   - CircleCollider2D (Is Trigger = true, Radius = 0.1)
   - Rigidbody2D (Body Type = Kinematic, Gravity Scale = 0)
   - EnemyProjectile (скрипт)
3. **Настройте EnemyProjectile**:
   - Damage = 10
   - Speed = 8
   - Lifetime = 5
4. **Сохраните как префаб**

---

## Шаг 4: Обновить EnemySpawner

Добавьте префабы врагов в список `enemyPrefabs` у EnemySpawner в сцене.

---

## Типы врагов - Характеристики

| Тип | HP | Скорость | Урон | Особенность |
|-----|-----|----------|------|-------------|
| Common | 20 | 2 | 5 | Базовый |
| Shooting | 15 | 1.5 | 8 | Стреляет на дистанции 8m |
| Tank | 60 | 1 | 15 | Медленный танк |
| Soldier | 35 | 3 | 12 | Быстрый и сильный |
| Boss | 200 | 1.5 | 25 | Стреляет + много HP |
| Goblin | 10 | 4 | 3 | Очень быстрый слабак |
