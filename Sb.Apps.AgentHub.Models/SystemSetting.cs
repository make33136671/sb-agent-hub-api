using FreeSql.DataAnnotations;
using System.ComponentModel.DataAnnotations;

namespace Sb.Apps.AgentHub.Models
{
    /// <summary>
    /// 系统设置
    /// </summary>
    [Table(Name = "sb_agenthub_system_setting")]
    public class SystemSetting : IEntity
    {
        /// <summary>主键Id</summary>
        [Column(Name = "Id", IsPrimary = true, IsIdentity = true)]
        public int Id { get; set; }

        /// <summary>
        /// 系统设置分类
        /// </summary>
        [Column(Name = "CategoryId")]
        public int CategoryId { get; set; }
        /// <summary>关联的设置分类</summary>
        [Navigate(nameof(CategoryId))]
        public SystemSettingCategory? Category { get; set; }

        /// <summary>
        /// 设置系统键
        /// </summary>
        [Required, StringLength(100)]
        [Column(Name = "Key")]
        public required string Key { get; set; }

        /// <summary>
        /// 设置名称
        /// </summary>
        [Required, StringLength(100)]
        [Column(Name = "Name")]
        public required string Name { get; set; }


        /// <summary>
        /// 设置界面显示类型
        /// </summary>
        [Column(Name = "DisplayType", MapType = typeof(string))]
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
        public string DataContentConfig { get; set; } = null!;

        /// <summary>
        /// 显示排序
        /// </summary>
        [Column(Name = "Sort")]
        public int Sort { get; set; }

    }

    /// <summary>
    /// 系统设置显示类型
    /// </summary>
    public enum SystemSettingDisplayType
    {
        /// <summary>文本输入框</summary>
        Input = 0,
        /// <summary>多行文本输入框</summary>
        InputTextArea = 1,
        /// <summary>单选按钮组</summary>
        RadioGroup = 2,
        /// <summary>下拉选择</summary>
        Select = 3,
        /// <summary>多选框组</summary>
        CheckboxGroup = 4,
        /// <summary>开关</summary>
        Switch = 5,
        /// <summary>单图上传</summary>
        ImageUpload = 6,
        /// <summary>数字输入框</summary>
        InputNumber = 7,
        /// <summary>密码输入框</summary>
        InputPassword = 8,
        /// <summary>日期选择器</summary>
        DatePicker = 9,
        /// <summary>时间选择器</summary>
        TimePicker = 10,
        /// <summary>日期范围选择器</summary>
        RangePicker = 11,
        /// <summary>评分</summary>
        Rate = 12,
        /// <summary>自动完成</summary>
        AutoComplete = 13,
        /// <summary>提及输入</summary>
        Mentions = 14,
        /// <summary>树形选择</summary>
        TreeSelect = 15,
        /// <summary>图标选择器</summary>
        IconPicker = 16,
        /// <summary>多图上传</summary>
        MultiImageUpload = 17,
        /// <summary>文件上传</summary>
        FileUpload = 18,
    }
}
