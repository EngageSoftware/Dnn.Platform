﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.BulkInstall.DeployClient;

using System.IO.Abstractions;

using DotNetNuke.BulkInstall.DeployClient;

using Spectre.Console;
using Spectre.Console.Cli;

public class DeployInputTests
{
    [InlineData("", false)]
    [InlineData("/test", false)]
    [InlineData("https://test.com", true)]
    [Theory]
    public void Validate_TargetUri(string targetUri, bool isSuccess)
    {
        var input = TestHelpers.CreateDeployInput(targetUri);
        var validate = ValidateInput(input);

        validate.Successful.ShouldBe(isSuccess);

        validate.Message.ShouldBe(isSuccess ? null : "--target-uri must be a valid URI");
    }

    [InlineData(-1, false)]
    [InlineData(0, true)]
    [InlineData(1, true)]
    [Theory]
    public void Validate_InstallationStatusTimeout(int timeout, bool isSuccess)
    {
        var input = TestHelpers.CreateDeployInput(installationStatusTimeout: timeout);
        var validate = ValidateInput(input);

        validate.Successful.ShouldBe(isSuccess);

        validate.Message.ShouldBe(isSuccess ? null : "--installation-status-timeout must be non-negative");
    }

    [InlineData("", true)]
    [InlineData("='\'", false)]
    [InlineData("Dir/Blah", true)]
    [Theory]
    public void Validate_PackagesDirectoryPath(string packagesDirectoryPath, bool isSuccess)
    {
        var fileSystem = A.Fake<IFileSystem>();
        var currentDirectory = Directory.GetCurrentDirectory();
        A.CallTo(() => fileSystem.Directory.Exists("Dir/Blah")).Returns(true);
        A.CallTo(() => fileSystem.Directory.Exists(currentDirectory)).Returns(true);

        var input = TestHelpers.CreateDeployInput(packagesDirectoryPath: packagesDirectoryPath);
        var validate = ValidateInput(input, fileSystem);

        validate.Successful.ShouldBe(isSuccess);

        validate.Message.ShouldBe(isSuccess ? null : "--packages-directory must be a valid path");
    }

    [Fact]
    public void CanSearchRecursively()
    {
        var input = new DeployInput { Recurse = true };
        input.Recurse.ShouldBeTrue();
    }

    [InlineData(LogLevel.Trace, true)]
    [InlineData(LogLevel.Error, true)]
    [InlineData((LogLevel)7, false)]
    [InlineData((LogLevel)(-1), false)]
    [Theory]
    public void Validate_LogLevel(LogLevel logLevel, bool isSuccess)
    {
        var input = TestHelpers.CreateDeployInput(logLevel: logLevel);
        var validate = ValidateInput(input);

        validate.Successful.ShouldBe(isSuccess);

        validate.Message.ShouldBe(isSuccess ? null : "--log-level must be a valid log level");
    }

    private static ValidationResult ValidateInput(DeployInput input, IFileSystem? fileSystem = null)
    {
        if (fileSystem == null)
        {
            fileSystem = A.Fake<IFileSystem>();
            var currentDirectory = Directory.GetCurrentDirectory();
            A.CallTo(() => fileSystem.Directory.Exists(currentDirectory)).Returns(true);
        }

        var command = new DeployCommand(A.Fake<IDeployer>(), fileSystem);
        return command.Validate(A.Dummy<CommandContext>(), input);
    }
}