using System.Collections.Generic;
using UnityEngine;

namespace CookingGame
{
    public class RecipeDatabase : MonoBehaviour
    {
        public List<Recipe> recipes = new List<Recipe>();

        private void Awake()
        {
            if (recipes == null || recipes.Count == 0)
            {
                InitializeDatabase();
            }
        }

        public void InitializeDatabase()
        {
            recipes = new List<Recipe>();

            // Recipe 1: ข้าวไรซ์เบอร์รี่ + อกไก่สมุนไพร
            recipes.Add(Recipe.CreateInstance(
                "ข้าวไรซ์เบอร์รี่ + อกไก่สมุนไพร",
                "Riceberry + Herbal Chicken Breast",
                "เมนูสุขภาพที่มีคาร์โบไฮเดรตเชิงซ้อนจากข้าวไรซ์เบอร์รี่ และโปรตีนลีนสูงจากอกไก่ ปรุงด้วยสมุนไพรหอมๆ ช่วยกระตุ้นระบบเผาผลาญ",
                new List<RecipeStep>()
                {
                    new RecipeStep("หุงข้าวไรซ์เบอร์รี่ (คนน้ำไม่ให้ติดหม้อ)", MinigameType.Stirring, 12f, "ข้าวไรซ์เบอร์รี่", "Food_Rice", "Food_Cooked Rice", "Prop_Pot_01"),
                    new RecipeStep("สับกระเทียมและโรสแมรี่", MinigameType.Chopping, 10f, "กระเทียม & โรสแมรี่", "Prop_Knife_01"),
                    new RecipeStep("ย่างอกไก่บนกระทะ (กลับด้านเมื่อสุกเป็นสีเหลืองทอง)", MinigameType.Grilling, 15f, "อกไก่", "Food_Chicken Breast", "Food_Seared Chicken", "Prop_Griller")
                },
                "Food_Seared Chicken"
            ));

            // Recipe 2: ต้มยำน้ำใสปลา + เห็ดรวม
            recipes.Add(Recipe.CreateInstance(
                "ต้มยำน้ำใสปลา + เห็ดรวม",
                "Clear Fish Tom Yum + Mixed Mushrooms",
                "ต้มยำปลาสมุนไพรน้ำใส รสชาติจัดจ้านแต่อบอุ่นท้อง ปราศจากไขมันส่วนเกิน มีเห็ดรวมช่วยเสริมสร้างภูมิคุ้มกัน",
                new List<RecipeStep>()
                {
                    new RecipeStep("หั่นเนื้อปลากะพงและเห็ด", MinigameType.Chopping, 12f, "ปลากะพง & เห็ด", "Prop_Knife_01"),
                    new RecipeStep("ต้มสมุนไพร (ข่า ตะไคร้ ใบมะกรูด) ในหม้อ", MinigameType.Stirring, 10f, "เครื่องต้มยำ", "Prop_Pot_01"),
                    new RecipeStep("บีบมะนาวและใส่น้ำปลา Low Sodium ปรุงรสตามชอบ", MinigameType.Seasoning, 12f, "เครื่องปรุงต้มยำ", "Prop_Pot_01")
                },
                "Food_Carrot Soup" // Placeholder soup prefab
            ));

            // Recipe 3: แกงเลียงกุ้งสด
            recipes.Add(Recipe.CreateInstance(
                "แกงเลียงกุ้งสด",
                "Herbal Spicy Soup with Fresh Shrimp",
                "เมนูสมุนไพรไทยพื้นบ้าน มีความเผ็ดร้อนจากพริกไทยดำ บำรุงร่างกายด้วยผักหลากหลายชนิด เช่น ฟักทอง บวบ และใบแมงลัก",
                new List<RecipeStep>()
                {
                    new RecipeStep("หั่นฟักทอง บวบ และข้าวโพดอ่อน", MinigameType.Chopping, 15f, "ผักแกงเลียง", "Prop_Knife_01"),
                    new RecipeStep("โขลกหอมแดง พริกไทย และกะปิให้เข้ากัน", MinigameType.Pounding, 12f, "พริกแกงเลียง", "Prop_WhipMixer"), // Mortar visual
                    new RecipeStep("ต้มน้ำซุป ใส่กุ้งสด ผักต่างๆ และใบแมงลัก", MinigameType.Stirring, 15f, "แกงเลียง", "Prop_Pot_01")
                },
                "Food_Carrot Soup"
            ));

            // Recipe 4: ปลาทูย่าง + น้ำพริกผักลวก
            recipes.Add(Recipe.CreateInstance(
                "ปลาทูย่าง + น้ำพริกผักลวก",
                "Grilled Mackerel + Chili Paste with Boiled Veggies",
                "เมนูคลาสสิกของไทย ปลาทูให้กรดไขมันดีโอเมก้า 3 ทานคู่กับน้ำพริกกะปิรสเด็ดและผักลวกหลากสี ดีต่อระบบขับถ่าย",
                new List<RecipeStep>()
                {
                    new RecipeStep("ย่างปลาทูให้สุกหอมทั้งสองด้าน", MinigameType.Grilling, 15f, "ปลาทู", "Prop_Griller"),
                    new RecipeStep("ต้มบล็อกโคลี่และแครอทในน้ำเดือด", MinigameType.Stirring, 12f, "ผักลวก", "Food_Broccoli", "Food_Carrot Soup", "Prop_Pot_01"),
                    new RecipeStep("ตำน้ำพริกด้วยพริก กระเทียม กะปิ และมะนาว", MinigameType.Pounding, 12f, "น้ำพริกกะปิ")
                },
                "Food_Cooked Steak" // Mackerel placeholder
            ));

            // Recipe 5: สุกี้น้ำไก่ผักเยอะ
            recipes.Add(Recipe.CreateInstance(
                "สุกี้น้ำไก่ผักเยอะ",
                "Chicken Suki Soup with Veggies",
                "สุกี้น้ำอุดมไปด้วยใยอาหารจากผักกาดขาวและผักบุ้ง โปรตีนลีนจากอกไก่และไข่ พร้อมน้ำจิ้มสูตรโซเดียมต่ำเพื่อสุขภาพ",
                new List<RecipeStep>()
                {
                    new RecipeStep("หั่นอกไก่ ผักกาดขาว และผักบุ้ง", MinigameType.Chopping, 15f, "อกไก่ & ผัก", "Prop_Knife_01"),
                    new RecipeStep("ต้มน้ำซุป ใส่ไก่ ผัก วุ้นเส้น และตีไข่ใส่ลงไป", MinigameType.Stirring, 15f, "สุกี้", "Prop_Pot_01"),
                    new RecipeStep("ราดน้ำจิ้มสุกี้สุขภาพตามปริมาณที่กำหนด", MinigameType.Seasoning, 10f, "น้ำจิ้มสุกี้")
                },
                "Food_Carrot Soup"
            ));

            // Recipe 6: ยำอกไก่แอปเปิ้ลเขียว
            recipes.Add(Recipe.CreateInstance(
                "ยำอกไก่แอปเปิ้ลเขียว",
                "Spicy Chicken Breast Salad with Green Apple",
                "ยำรสเด็ดที่ใช้ความเปรี้ยวธรรมชาติจากแอปเปิ้ลเขียวไขมันต่ำ เพิ่มความหอมสดชื่นด้วยสะระแหน่และผักชี",
                new List<RecipeStep>()
                {
                    new RecipeStep("หั่นอกไก่ต้มและหั่นแอปเปิ้ลเขียวเป็นเต๋า", MinigameType.Chopping, 15f, "อกไก่ & แอปเปิ้ลเขียว", "Prop_Knife_01"),
                    new RecipeStep("ผสมน้ำยำ: บีบมะนาว น้ำปลา Low-Sodium และพริก", MinigameType.Seasoning, 12f, "น้ำยำ", "Prop_Bowel_01"),
                    new RecipeStep("คลุกเคล้าส่วนผสมทั้งหมดเข้าด้วยกันอย่างเบามือ", MinigameType.Stirring, 10f, "ยำอกไก่", "Prop_Bowel_01")
                },
                "Food_Salad"
            ));

            // Recipe 7: แกงส้มผักรวมปลา
            recipes.Add(Recipe.CreateInstance(
                "แกงส้มผักรวมปลา",
                "Sour Curry with Mixed Vegetables and Fish",
                "แกงไทยไร้น้ำมัน ปรุงรสเปรี้ยวด้วยน้ำมะขามเปียกธรรมชาติ ใส่ผักรวมที่มีไฟเบอร์สูงและเนื้อปลาสีขาวไขมันต่ำ",
                new List<RecipeStep>()
                {
                    new RecipeStep("หั่นดอกกะหล่ำ แครอท และถั่วฝักยาว", MinigameType.Chopping, 12f, "ผักแกงส้ม", "Prop_Knife_01"),
                    new RecipeStep("ละลายน้ำพริกแกงส้มในหม้อต้มให้เดือด", MinigameType.Stirring, 10f, "น้ำแกงส้ม", "Prop_Pot_01"),
                    new RecipeStep("ใส่ปลาขาวและผักลงไปต้มจนสุกทั่วกัน", MinigameType.Stirring, 12f, "แกงส้ม", "Prop_Pot_01")
                },
                "Food_Carrot Soup"
            ));

            // Recipe 8: ข้าวกล้อง + ผัดบล็อกโคลี่เห็ดหอม
            recipes.Add(Recipe.CreateInstance(
                "ข้าวกล้อง + ผัดบล็อกโคลี่เห็ดหอม",
                "Brown Rice + Stir-fried Broccoli with Shiitake",
                "อาหารจานเดียวที่ได้คาร์โบไฮเดรตเชิงซ้อนจากข้าวกล้อง ผัดผักบล็อกโคลี่กรุบกรอบและเห็ดหอมในน้ำมันมะกอกปริมาณเล็กน้อย",
                new List<RecipeStep>()
                {
                    new RecipeStep("หั่นบล็อกโคลี่ แครอท และเห็ดหอม", MinigameType.Chopping, 12f, "ผักผัด", "Prop_Knife_01"),
                    new RecipeStep("ผัดกระเทียม เห็ดหอม และผักในน้ำมันมะกอก", MinigameType.Stirring, 15f, "ผัดบล็อกโคลี่", "Prop_Pan_01"),
                    new RecipeStep("เหยาะซอสปรุงรส Low Sodium ให้ได้รสชาติพอดี", MinigameType.Seasoning, 10f, "ซอสปรุงรส", "Prop_Pan_01")
                },
                "Food_Mac n Cheese with brocoli" // stir fry placeholder
            ));

            // Recipe 9: ลาบปลาไม่ใส่น้ำตาล
            recipes.Add(Recipe.CreateInstance(
                "ลาบปลาไม่ใส่น้ำตาล",
                "Spicy Fish Salad (Larb, Sugar-Free)",
                "ลาบอีสานปรุงแบบคลีน ใช้เนื้อปลากะพงสับละเอียด คั่วในกระทะ รสเปรี้ยวเค็มเผ็ดหอมกลิ่นข้าวคั่วและสมุนไพรสด",
                new List<RecipeStep>()
                {
                    new RecipeStep("สับเนื้อปลากะพงให้ละเอียด", MinigameType.Chopping, 12f, "เนื้อปลากะพง", "Prop_Knife_01"),
                    new RecipeStep("รวนเนื้อปลาสับในกระทะให้สุกโดยไม่ใช้น้ำมัน", MinigameType.Stirring, 10f, "เนื้อปลา", "Prop_Pan_01"),
                    new RecipeStep("ใส่พริกป่น มะนาว น้ำปลา Low Sodium และข้าวคั่ว", MinigameType.Seasoning, 12f, "เครื่องลาบ", "Prop_Bowel_01")
                },
                "Food_Salad"
            ));

            // Recipe 10: ข้าวต้มปลาเห็ดหอมขิงอ่อน
            recipes.Add(Recipe.CreateInstance(
                "ข้าวต้มปลาเห็ดหอมขิงอ่อน",
                "Fish Rice Soup with Shiitake & Ginger",
                "มื้อเช้าแสนอบอุ่น ขิงอ่อนซอยช่วยขับลม เนื้อปลากะพงสดต้มในซุปใสเห็ดหอมกระตุ้นพลังงานยามเช้าอย่างดี",
                new List<RecipeStep>()
                {
                    new RecipeStep("หั่นขิงอ่อน เห็ดหอม และหั่นชิ้นปลากะพง", MinigameType.Chopping, 15f, "ปลา & สมุนไพร", "Prop_Knife_01"),
                    new RecipeStep("ต้มน้ำซุปกระดูกปลากับขิงซอยและเห็ดหอม", MinigameType.Stirring, 12f, "น้ำซุปข้าวต้ม", "Prop_Pot_01"),
                    new RecipeStep("ใส่ข้าวหอมมะลิและเนื้อปลาลงต้มจนสุกนุ่ม", MinigameType.Stirring, 15f, "ข้าวต้มปลา", "Prop_Pot_01")
                },
                "Food_Carrot Soup"
            ));
        }

        public Recipe GetRecipe(int index)
        {
            if (recipes == null || recipes.Count == 0) InitializeDatabase();
            if (index >= 0 && index < recipes.Count) return recipes[index];
            return null;
        }
    }
}
