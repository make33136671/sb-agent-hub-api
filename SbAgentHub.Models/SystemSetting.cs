using FreeSql.DataAnnotations;
using System.ComponentModel.DataAnnotations;

namespace SbAgentHub.Models
{
    /// <summary>
    /// 系统设置
    /// </summary>
    [Table(Name = "SB_SystemSetting")]
    public class SystemSetting : IEntity
    {
        [Column(Name = "Id", IsPrimary = true, IsIdentity = true)]
        public int Id { get; set; }

        /// <summary>
        /// 系统设置分类
        /// </summary>
        [Column(Name = "CategoryId")]
        public int CategoryId { get; set; }
        [Navigate(nameof(CategoryId))]
        public SystemSettingCategory? Category { get; set; }

        /// <summary>
        /// 设置系统键
        /// </summary>
        [Required, StringLength(100)]
        [Column(Name = "Key")]
        public string Key { get; set; }

        /// <summary>
        /// 设置名称
        /// </summary>
        [Required, StringLength(100)]
        [Column(Name = "Name")]
        public string Name { get; set; }


        /// <summary>
        /// 设置界面显示类型
        /// </summary>
        [Column(Name = "DisplayType")]
        public SystemSettingDisplayType DisplayType { get; set; }

        /// <summary>
        /// 系统设置不能删除
        /// </summary>
        [Column(Name = "CantDelete")]
        public bool CantDelete { get; set; }

        /// <summary>
        /// 设置数据内容
        /// </summary>
        [Column(Name = "DataContent")]
        public string? DataContent { get; set; }

        /// <summary>
        /// 设置数据选项
        /// </summary>
        [Column(Name = "DataContentConfig")]
        public string DataContentConfig { get; set; }

        /// <summary>
        /// 显示排序
        /// </summary>
        [Column(Name = "Sort")]
        public int Sort { get; set; }

    }

    public enum SystemSettingDisplayType
    {
        Input = 0,              // 文本输入框
        InputTextArea = 1,      // 多行文本输入框
        RadioGroup = 2,         // 单选按钮组
        Select = 3,             // 下拉选择
        CheckboxGroup = 4,      // 多选框组
        Switch = 5,             // 开关
        ImageUpload = 6,        // 单图上传
        InputNumber = 7,        // 数字输入框
        InputPassword = 8,      // 密码输入框
        DatePicker = 9,         // 日期选择器
        TimePicker = 10,        // 时间选择器
        RangePicker = 11,       // 日期范围选择器
        Rate = 12,              // 评分
        AutoComplete = 13,      // 自动完成
        Mentions = 14,          // 提及输入
        TreeSelect = 15,        // 树形选择
        IconPicker = 16,        // 图标选择器
        MultiImageUpload = 17,  // 多图上传
        FileUpload = 18,        // 文件上传
    }
}