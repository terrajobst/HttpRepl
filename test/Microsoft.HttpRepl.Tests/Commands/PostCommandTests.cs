// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.HttpRepl.Commands;
using Microsoft.HttpRepl.Fakes;
using Microsoft.HttpRepl.Preferences;
using Microsoft.HttpRepl.Resources;
using Microsoft.Repl.ConsoleHandling;
using Microsoft.Repl.Parsing;
using Xunit;

namespace Microsoft.HttpRepl.Tests.Commands
{
    public class PostCommandTests : CommandTestsBase
    {
        private string _baseAddress;
        private string _testPath;
        private string _noBodyRequiredPath;
        private IDictionary<string, string> _urlsWithResponse = new Dictionary<string, string>();

        public PostCommandTests()
        {
            _baseAddress = "http://localhost:5050/";
            _testPath = "this/is/a/test/route";
            _noBodyRequiredPath = "no/body/required";

            _urlsWithResponse.Add(_baseAddress, "This is a test response from a POST: \"Test Post Body\"");
            _urlsWithResponse.Add(_baseAddress + _testPath, "This is a test response from a POST: \"Test Post Body\"");
            _urlsWithResponse.Add(_baseAddress + _noBodyRequiredPath, "This is a test response from a POST: \"\"");
        }

        [Fact]
        public async Task ExecuteAsync_WithNoBasePath_VerifyError()
        {
            ArrangeInputs(commandText: "POST",
                baseAddress: null,
                path: null,
                urlsWithResponse: null,
                out MockedShellState shellState,
                out HttpState httpState,
                out ICoreParseResult parseResult,
                out MockedFileSystem fileSystem,
                out IPreferences preferences);

            string expectedErrorMessage = Strings.Error_NoBasePath.SetColor(httpState.ErrorColor);

            PostCommand postCommand = new PostCommand(fileSystem, preferences);
            await postCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            Assert.Equal(expectedErrorMessage, shellState.ErrorMessage);
        }

        [Fact]
        public async Task ExecuteAsync_OnlyBaseAddressWithInlineContent_VerifyResponse()
        {
            ArrangeInputs(commandText: "POST --content \"Test Post Body\"",
                baseAddress: _baseAddress,
                path: _testPath,
                urlsWithResponse: _urlsWithResponse,
                out MockedShellState shellState,
                out HttpState httpState,
                out ICoreParseResult parseResult,
                out MockedFileSystem fileSystem,
                out IPreferences preferences);

            PostCommand postCommand = new PostCommand(fileSystem, preferences);
            await postCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            string expectedResponse = "This is a test response from a POST: \"Test Post Body\"";
            List<string> result = shellState.Output;

            Assert.Equal(2, result.Count);
            Assert.Contains("HTTP/1.1 200 OK", result);
            Assert.Contains(expectedResponse, result);
        }

        [Fact]
        public async Task ExecuteAsync_MultiPartRouteWithInlineContent_VerifyResponse()
        {
            ArrangeInputs(commandText: "POST --content \"Test Post Body\"",
                baseAddress: _baseAddress,
                path: _testPath,
                urlsWithResponse: _urlsWithResponse,
                out MockedShellState shellState,
                out HttpState httpState,
                out ICoreParseResult parseResult,
                out MockedFileSystem fileSystem,
                out IPreferences preferences);

            PostCommand postCommand = new PostCommand(fileSystem, preferences);
            await postCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            string expectedResponse = "This is a test response from a POST: \"Test Post Body\"";
            List<string> result = shellState.Output;

            Assert.Equal(2, result.Count);
            Assert.Contains("HTTP/1.1 200 OK", result);
            Assert.Contains(expectedResponse, result);
        }

        [Fact]
        public async Task ExecuteAsync_MultiPartRouteWithNoBodyRequired_VerifyResponse()
        {
            ArrangeInputs(commandText: "POST --no-body",
                baseAddress: _baseAddress,
                path: _noBodyRequiredPath,
                urlsWithResponse: _urlsWithResponse,
                out MockedShellState shellState,
                out HttpState httpState,
                out ICoreParseResult parseResult,
                out MockedFileSystem fileSystem,
                out IPreferences preferences);

            PostCommand postCommand = new PostCommand(fileSystem, preferences);
            await postCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            string expectedResponse = "This is a test response from a POST: \"\"";
            List<string> result = shellState.Output;

            Assert.Equal(2, result.Count);
            Assert.Contains("HTTP/1.1 200 OK", result);
            Assert.Contains(expectedResponse, result);
        }

        [Fact]
        public async Task ExecuteAsync_MultiPartRouteWithBodyFromFile_VerifyResponse()
        {
            string filePath = "someFilePath.txt";
            string fileContents = "This is a test response from a POST: \"Test Post Body From File\"";

            ArrangeInputs(commandText: $"POST --file " + filePath,
                baseAddress: _baseAddress,
                path: _testPath,
                urlsWithResponse: _urlsWithResponse,
                out MockedShellState shellState,
                out HttpState httpState,
                out ICoreParseResult parseResult,
                out MockedFileSystem fileSystem,
                out IPreferences preferences,
                readBodyFromFile: true,
                fileContents: fileContents);

            fileSystem.AddFile(filePath, "Test Post Body From File");

            PostCommand postCommand = new PostCommand(fileSystem, preferences);
            await postCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            List<string> result = shellState.Output;

            Assert.Equal(2, result.Count);
            Assert.Contains("HTTP/1.1 200 OK", result);
            Assert.Contains(fileContents, result);
        }
    }
}
