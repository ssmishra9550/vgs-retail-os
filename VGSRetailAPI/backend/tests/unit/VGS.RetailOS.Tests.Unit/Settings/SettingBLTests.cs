using Moq;
using VGS.RetailOS.Contracts.V1.Settings.Requests;
using VGS.RetailOS.Modules.Settings.Setting.BL;
using VGS.RetailOS.Modules.Settings.Setting.BO;
using VGS.RetailOS.Modules.Settings.Setting.IDAC;
using VGS.RetailOS.Shared.Tenancy;
using Xunit;

namespace VGS.RetailOS.Tests.Unit.Settings;

public class SettingBLTests
{
    private readonly Mock<ISettingDAC> _settingDacMock;
    private readonly Mock<ITenantContextAccessor> _tenantContextAccessorMock;
    private readonly SettingBL _sut;

    public SettingBLTests()
    {
        _settingDacMock = new Mock<ISettingDAC>();
        _tenantContextAccessorMock = new Mock<ITenantContextAccessor>();

        _tenantContextAccessorMock.Setup(x => x.TenantContext).Returns(new TenantContext("tenant-1"));

        _sut = new SettingBL(_settingDacMock.Object, _tenantContextAccessorMock.Object);
    }

    [Fact]
    public async Task GetSettingAsync_ShouldReturnStoreSetting_WhenStoreSettingExists()
    {
        // Arrange
        var storeId = Guid.NewGuid();
        var key = "Currency";

        _settingDacMock.Setup(x => x.GetSettingAsync(key, "tenant-1", storeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SettingBO { Key = key, Value = "EUR", StoreId = storeId });

        // Act
        var result = await _sut.GetSettingAsync(key, storeId, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("EUR", result.Value);
    }

    [Fact]
    public async Task GetSettingAsync_ShouldFallbackToTenantSetting_WhenStoreSettingDoesNotExist()
    {
        // Arrange
        var storeId = Guid.NewGuid();
        var key = "Currency";

        _settingDacMock.Setup(x => x.GetSettingAsync(key, "tenant-1", storeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SettingBO?)null);

        _settingDacMock.Setup(x => x.GetSettingAsync(key, "tenant-1", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SettingBO { Key = key, Value = "USD", StoreId = null });

        // Act
        var result = await _sut.GetSettingAsync(key, storeId, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("USD", result.Value);
    }

    [Fact]
    public async Task GetAllSettingsAsync_ShouldOverrideTenantSettingsWithStoreSettings()
    {
        // Arrange
        var storeId = Guid.NewGuid();

        var settings = new List<SettingBO>
        {
            new SettingBO { Key = "Currency", Value = "USD", StoreId = null }, // Global
            new SettingBO { Key = "Currency", Value = "EUR", StoreId = storeId }, // Store override
            new SettingBO { Key = "Timezone", Value = "UTC", StoreId = null } // Global only
        };

        _settingDacMock.Setup(x => x.GetAllSettingsAsync("tenant-1", storeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(settings);

        // Act
        var result = await _sut.GetAllSettingsAsync(storeId, CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Count);
        var currencySetting = result.First(s => s.Key == "Currency");
        var timezoneSetting = result.First(s => s.Key == "Timezone");

        Assert.Equal("EUR", currencySetting.Value); // Store override applies
        Assert.Equal("UTC", timezoneSetting.Value);
    }

    [Fact]
    public async Task UpsertSettingAsync_ShouldUpsert()
    {
        // Arrange
        var request = new UpsertSettingRequest { Key = "Theme", Value = "Dark", Group = "UI" };

        _settingDacMock.Setup(x => x.UpsertSettingAsync(It.IsAny<SettingBO>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((SettingBO b, CancellationToken c) => b);

        // Act
        var result = await _sut.UpsertSettingAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Theme", result.Key);
        Assert.Equal("Dark", result.Value);
    }
}
