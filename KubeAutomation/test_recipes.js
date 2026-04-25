// test_recipes.js
ServerEvents.recipes(event => 
{
    // Удалить старые рецепты мода
    event.remove({ mod: "old_mod" })

    /*
     * Рецепты Create Mod
     */
    event.custom({
        type: "create:sequenced_assembly"
    })

    // Удалить рецепты по выходу
    event.remove({ output: "minecraft:stone_pickaxe" })

    // Удалить по сложному фильтру (с NOT)
    event.remove({ output: "stone", not: { type: "minecraft:smelting" } })

    testMyPatience();

    event.recipes.createCompacting("tfmg:cast_iron_ingot", "tfmg:cast_iron_block");

    //1. Массив объектов
    const biomassValues = [
        { input: 'minecraft:kelp',          count: 2 },
        { input: 'minecraft:seagrass',      count: 2 },
        { input: 'minecraft:potato',        count: 3 },
        { input: 'minecraft:carrot',        count: 3 },
        //{ input: '#minecraft:saplings',     count: 4 }, // Можно использовать теги как вход! Но не для машин 
        { input: 'minecraft:wheat',         count: 2 },
        { input: 'create:wheat_flour',      count: 1 },
        { input: 'minecraft:wheat_seeds',   count: 9 },
        { input: 'minecraft:sugar_cane',    count: 2 },
        { input: 'minecraft:cactus',        count: 1 }
    ];

    // 2. Генерация рецептов в цикле
    biomassValues.forEach(data => {  
        event.recipes.create.compacting('kubejs:biomass', Item.of(data.input, data.count)); 
    });

    event.recipes.create.filling('minecraft:water_bucket', 
        ['minecraft:bucket', Fluid.of('minecraft:water', 1000)])

    event.recipes.create.crushing(
        ['minecraft:diamond', CreateItem.of('minecraft:emerald', 0.5)], 
        'minecraft:coal_block')
        .processingTime(500)

    event.recipes.create.deploying(
        'minecraft:diamond', ['minecraft:coal_block', 'minecraft:sand'])
        .keepHeldItem()
    // Конец файла
});