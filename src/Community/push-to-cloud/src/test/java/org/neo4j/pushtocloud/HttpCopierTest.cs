using System;

/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
namespace Neo4Net.Pushtocloud
{
	using MappingBuilder = com.github.tomakehurst.wiremock.client.MappingBuilder;
	using ResponseDefinitionBuilder = com.github.tomakehurst.wiremock.client.ResponseDefinitionBuilder;
	using WireMockRule = com.github.tomakehurst.wiremock.junit.WireMockRule;
	using Scenario = com.github.tomakehurst.wiremock.stubbing.Scenario;
	using CoreMatchers = org.hamcrest.CoreMatchers;
	using Matcher = org.hamcrest.Matcher;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using Mockito = org.mockito.Mockito;


	using CommandFailed = Neo4Net.CommandLine.Admin.CommandFailed;
	using ProgressListener = Neo4Net.Helpers.progress.ProgressListener;
	using DefaultFileSystemAbstraction = Neo4Net.Io.fs.DefaultFileSystemAbstraction;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.github.tomakehurst.wiremock.client.WireMock.aResponse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.github.tomakehurst.wiremock.client.WireMock.containing;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.github.tomakehurst.wiremock.client.WireMock.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.github.tomakehurst.wiremock.client.WireMock.get;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.github.tomakehurst.wiremock.client.WireMock.matching;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.github.tomakehurst.wiremock.client.WireMock.notMatching;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.github.tomakehurst.wiremock.client.WireMock.post;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.github.tomakehurst.wiremock.client.WireMock.postRequestedFor;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.github.tomakehurst.wiremock.client.WireMock.put;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.github.tomakehurst.wiremock.client.WireMock.putRequestedFor;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.github.tomakehurst.wiremock.client.WireMock.urlEqualTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.github.tomakehurst.wiremock.client.WireMock.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.allOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Matchers.anyLong;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.atLeast;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.pushtocloud.HttpCopier.HTTP_RESUME_INCOMPLETE;

	public class HttpCopierTest
	{
		 private static readonly HttpCopier.ProgressListenerFactory _noOpProgress = ( name, length ) => Neo4Net.Helpers.progress.ProgressListener_Fields.None;

		 private const int TEST_PORT = 8080;
		 private static readonly string _testConsoleUrl = "http://localhost:" + TEST_PORT;
		 private const string STATUS_POLLING_PASSED_FIRST_CALL = "Passed first";

		 private readonly DefaultFileSystemAbstraction _fs = new DefaultFileSystemAbstraction();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public com.github.tomakehurst.wiremock.junit.WireMockRule wireMock = new com.github.tomakehurst.wiremock.junit.WireMockRule(TEST_PORT);
		 public WireMockRule WireMock = new WireMockRule( TEST_PORT );
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.TestDirectory directory = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public TestDirectory Directory = TestDirectory.testDirectory();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleSuccessfulHappyCaseRunThroughOfTheWholeProcess() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleSuccessfulHappyCaseRunThroughOfTheWholeProcess()
		 {
			  // given
			  ControlledProgressListener progressListener = new ControlledProgressListener();
			  HttpCopier copier = new HttpCopier(new ControlledOutsideWorld(_fs), millis =>
			  {
			  }, ( name, length ) => progressListener);
			  Path source = CreateDump();
			  long sourceLength = _fs.getFileSize( source.toFile() );

			  string authorizationTokenResponse = "abc";
			  string signedURIPath = "/signed";
			  string uploadLocationPath = "/upload";
			  WireMock.stubFor( AuthenticationRequest( false ).willReturn( SuccessfulAuthorizationResponse( authorizationTokenResponse ) ) );
			  WireMock.stubFor( InitiateUploadTargetRequest( authorizationTokenResponse ).willReturn( SuccessfulInitiateUploadTargetResponse( signedURIPath ) ) );
			  WireMock.stubFor( InitiateUploadRequest( signedURIPath ).willReturn( SuccessfulInitiateUploadResponse( uploadLocationPath ) ) );
			  WireMock.stubFor( ResumeUploadRequest( uploadLocationPath, sourceLength ).willReturn( SuccessfulResumeUploadResponse() ) );
			  WireMock.stubFor( TriggerImportRequest( authorizationTokenResponse ).willReturn( SuccessfulTriggerImportResponse() ) );
			  WireMock.stubFor( FirstStatusPollingRequest( authorizationTokenResponse ) );
			  WireMock.stubFor( SecondStatusPollingRequest( authorizationTokenResponse ) );

			  // when
			  AuthenticateAndCopy( copier, source, "user", "pass".ToCharArray() );

			  // then
			  verify( postRequestedFor( urlEqualTo( "/import/auth" ) ) );
			  verify( postRequestedFor( urlEqualTo( "/import" ) ) );
			  verify( postRequestedFor( urlEqualTo( signedURIPath ) ) );
			  verify( putRequestedFor( urlEqualTo( uploadLocationPath ) ) );
			  verify( postRequestedFor( urlEqualTo( "/import/upload-complete" ) ) );
			  assertTrue( progressListener.DoneCalled );
			  // we need to add 3 to the progress listener because of the database phases
			  assertEquals( sourceLength + 3, progressListener.Progress );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleBadCredentialsInAuthorizationRequest() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleBadCredentialsInAuthorizationRequest()
		 {
			  // given
			  HttpCopier copier = new HttpCopier( new ControlledOutsideWorld( _fs ) );
			  Path source = CreateDump();
			  WireMock.stubFor( AuthenticationRequest( false ).willReturn( aResponse().withStatus(HTTP_UNAUTHORIZED) ) );

			  // when/then
			  AssertThrows( typeof( CommandFailed ), CoreMatchers.equalTo( "Invalid username/password credentials" ), () => authenticateAndCopy(copier, source, "user", "pass".ToCharArray()) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleAuthenticateMovedRoute() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleAuthenticateMovedRoute()
		 {
			  // given
			  HttpCopier copier = new HttpCopier( new ControlledOutsideWorld( _fs ) );
			  Path source = CreateDump();
			  WireMock.stubFor( AuthenticationRequest( false ).willReturn( aResponse().withStatus(HTTP_NOT_FOUND) ) );

			  // when/then
			  AssertThrows( typeof( CommandFailed ), CoreMatchers.containsString( "please contact support" ), () => authenticateAndCopy(copier, source, "user", "pass".ToCharArray()) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleMoveUploadTargetdRoute() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleMoveUploadTargetdRoute()
		 {
			  // given
			  HttpCopier copier = new HttpCopier( new ControlledOutsideWorld( _fs ) );
			  Path source = CreateDump();
			  long sourceLength = _fs.getFileSize( source.toFile() );

			  string authorizationTokenResponse = "abc";
			  string signedURIPath = "/signed";
			  string uploadLocationPath = "/upload";

			  WireMock.stubFor( AuthenticationRequest( false ).willReturn( SuccessfulAuthorizationResponse( authorizationTokenResponse ) ) );
			  WireMock.stubFor( InitiateUploadRequest( signedURIPath ).willReturn( SuccessfulInitiateUploadResponse( uploadLocationPath ) ) );
			  WireMock.stubFor( InitiateUploadTargetRequest( "abc" ).willReturn( aResponse().withStatus(HTTP_NOT_FOUND) ) );

			  // when/then
			  AssertThrows( typeof( CommandFailed ), CoreMatchers.containsString( "please contact support" ), () => authenticateAndCopy(copier, source, "user", "pass".ToCharArray()) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleImportRequestestMovedRoute() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleImportRequestestMovedRoute()
		 {
			  // given
			  HttpCopier copier = new HttpCopier( new ControlledOutsideWorld( _fs ) );
			  Path source = CreateDump();
			  long sourceLength = _fs.getFileSize( source.toFile() );

			  string authorizationTokenResponse = "abc";
			  string signedURIPath = "/signed";
			  string uploadLocationPath = "/upload";

			  WireMock.stubFor( AuthenticationRequest( false ).willReturn( SuccessfulAuthorizationResponse( authorizationTokenResponse ) ) );
			  WireMock.stubFor( InitiateUploadTargetRequest( authorizationTokenResponse ).willReturn( SuccessfulInitiateUploadTargetResponse( signedURIPath ) ) );
			  WireMock.stubFor( InitiateUploadRequest( signedURIPath ).willReturn( SuccessfulInitiateUploadResponse( uploadLocationPath ) ) );
			  WireMock.stubFor( ResumeUploadRequest( uploadLocationPath, sourceLength ).willReturn( SuccessfulResumeUploadResponse() ) );

			  WireMock.stubFor( TriggerImportRequest( "abc" ).willReturn( aResponse().withStatus(HTTP_NOT_FOUND) ) );

			  // when/then
			  AssertThrows( typeof( CommandFailed ), CoreMatchers.containsString( "please contact support" ), () => authenticateAndCopy(copier, source, "user", "pass".ToCharArray()) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleInsufficientCredentialsInAuthorizationRequest() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleInsufficientCredentialsInAuthorizationRequest()
		 {
			  // given
			  HttpCopier copier = new HttpCopier( new ControlledOutsideWorld( _fs ) );
			  Path source = CreateDump();
			  WireMock.stubFor( AuthenticationRequest( false ).willReturn( aResponse().withStatus(HTTP_FORBIDDEN) ) );

			  // when/then
			  AssertThrows( typeof( CommandFailed ), containsString( "administrative access" ), () => authenticateAndCopy(copier, source, "user", "pass".ToCharArray()) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleUnexpectedResponseFromAuthorizationRequest() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleUnexpectedResponseFromAuthorizationRequest()
		 {
			  // given
			  HttpCopier copier = new HttpCopier( new ControlledOutsideWorld( _fs ) );
			  Path source = CreateDump();
			  WireMock.stubFor( AuthenticationRequest( false ).willReturn( aResponse().withStatus(HTTP_INTERNAL_ERROR) ) );

			  // when/then
			  AssertThrows( typeof( CommandFailed ), allOf( containsString( "Unexpected response" ), containsString( "Authorization" ) ), () => authenticateAndCopy(copier, source, "user", "pass".ToCharArray()) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleUnauthorizedResponseFromInitiateUploadTarget() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleUnauthorizedResponseFromInitiateUploadTarget()
		 {
			  HttpCopier copier = new HttpCopier( new ControlledOutsideWorld( _fs ) );
			  Path source = CreateDump();
			  string token = "abc";
			  WireMock.stubFor( AuthenticationRequest( false ).willReturn( SuccessfulAuthorizationResponse( token ) ) );
			  WireMock.stubFor( InitiateUploadTargetRequest( token ).willReturn( aResponse().withStatus(HTTP_UNAUTHORIZED) ) );

			  // when/then
			  AssertThrows( typeof( CommandFailed ), containsString( "authorization token is invalid" ), () => authenticateAndCopy(copier, source, "user", "pass".ToCharArray()) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleConflictResponseFromInitiateUploadTargetAndContinueOnUserConsent() throws java.io.IOException, org.neo4j.commandline.admin.CommandFailed
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleConflictResponseFromInitiateUploadTargetAndContinueOnUserConsent()
		 {
			  ControlledOutsideWorld outsideWorld = new ControlledOutsideWorld( _fs );
			  outsideWorld.WithPromptResponse( "my-username" ); // prompt for username
			  outsideWorld.WithPasswordResponse( "pass".ToCharArray() ); // prompt for password
			  outsideWorld.WithPromptResponse( "y" ); // prompt for consent to overwrite db
			  HttpCopier copier = new HttpCopier( outsideWorld );
			  Path source = CreateDump();
			  long sourceLength = _fs.getFileSize( source.toFile() );
			  string authorizationTokenResponse = "abc";
			  string signedURIPath = "/signed";
			  string uploadLocationPath = "/upload";
			  WireMock.stubFor( AuthenticationRequest( true ).willReturn( SuccessfulAuthorizationResponse( authorizationTokenResponse ) ) );
			  WireMock.stubFor( AuthenticationRequest( false ).willReturn( aResponse().withStatus(HTTP_CONFLICT) ) );
			  WireMock.stubFor( InitiateUploadTargetRequest( authorizationTokenResponse ).willReturn( SuccessfulInitiateUploadTargetResponse( signedURIPath ) ) );
			  // and just the rest of the responses so that the upload can continue w/o failing
			  WireMock.stubFor( InitiateUploadRequest( signedURIPath ).willReturn( SuccessfulInitiateUploadResponse( uploadLocationPath ) ) );
			  WireMock.stubFor( ResumeUploadRequest( uploadLocationPath, sourceLength ).willReturn( SuccessfulResumeUploadResponse() ) );
			  WireMock.stubFor( TriggerImportRequest( authorizationTokenResponse ).willReturn( SuccessfulTriggerImportResponse() ) );
			  WireMock.stubFor( FirstStatusPollingRequest( authorizationTokenResponse ) );
			  WireMock.stubFor( SecondStatusPollingRequest( authorizationTokenResponse ) );

			  // when
			  AuthenticateAndCopy( copier, source, "user", "pass".ToCharArray() );

			  // then there should be one request w/o the user consent and then (since the user entered 'y') one w/ user consent
			  verify( postRequestedFor( urlEqualTo( "/import/auth" ) ).withHeader( "Confirmed", equalTo( "false" ) ) );
			  verify( postRequestedFor( urlEqualTo( "/import/auth" ) ).withHeader( "Confirmed", equalTo( "true" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleConflictResponseFromAuthenticationWithoutUserConsent() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleConflictResponseFromAuthenticationWithoutUserConsent()
		 {
			  ControlledOutsideWorld outsideWorld = new ControlledOutsideWorld( _fs );
			  outsideWorld.WithPromptResponse( "my-username" ); // prompt for username
			  outsideWorld.WithPromptResponse( "n" ); // prompt for consent to overwrite db
			  HttpCopier copier = new HttpCopier( outsideWorld );
			  Path source = CreateDump();
			  string authorizationTokenResponse = "abc";
			  string signedURIPath = "/signed";
			  WireMock.stubFor( AuthenticationRequest( false ).willReturn( aResponse().withStatus(HTTP_CONFLICT) ) );
			  WireMock.stubFor( AuthenticationRequest( true ).willReturn( SuccessfulAuthorizationResponse( authorizationTokenResponse ) ) );
			  WireMock.stubFor( InitiateUploadTargetRequest( authorizationTokenResponse ).willReturn( SuccessfulInitiateUploadTargetResponse( signedURIPath ) ) );

			  // when
			  AssertThrows( typeof( CommandFailed ), containsString( "No consent to overwrite" ), () => authenticateAndCopy(copier, source, "user", "pass".ToCharArray()) );

			  // then there should be one request w/o the user consent and then (since the user entered 'y') one w/ user consent
			  verify( postRequestedFor( urlEqualTo( "/import/auth" ) ).withHeader( "Confirmed", equalTo( "false" ) ) );
			  verify( 0, postRequestedFor( urlEqualTo( "/import/auth" ) ).withHeader( "Confirmed", equalTo( "true" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleUnexpectedResponseFromInitiateUploadTargetRequest() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleUnexpectedResponseFromInitiateUploadTargetRequest()
		 {
			  ControlledOutsideWorld outsideWorld = new ControlledOutsideWorld( _fs );
			  outsideWorld.WithPromptResponse( "my-username" ); // prompt for username
			  outsideWorld.WithPromptResponse( "n" ); // prompt for consent to overwrite db
			  HttpCopier copier = new HttpCopier( outsideWorld );
			  Path source = CreateDump();
			  string authorizationTokenResponse = "abc";
			  WireMock.stubFor( AuthenticationRequest( false ).willReturn( SuccessfulAuthorizationResponse( authorizationTokenResponse ) ) );
			  WireMock.stubFor( InitiateUploadTargetRequest( authorizationTokenResponse ).willReturn( aResponse().withStatus(HTTP_BAD_GATEWAY) ) );

			  // when
			  AssertThrows( typeof( CommandFailed ), allOf( containsString( "Unexpected response" ), containsString( "Initiating upload target" ) ), () => authenticateAndCopy(copier, source, "user", "pass".ToCharArray()) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleInitiateUploadFailure() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleInitiateUploadFailure()
		 {
			  HttpCopier copier = new HttpCopier( new ControlledOutsideWorld( _fs ) );
			  Path source = CreateDump();
			  string authorizationTokenResponse = "abc";
			  string signedURIPath = "/signed";
			  WireMock.stubFor( AuthenticationRequest( false ).willReturn( SuccessfulAuthorizationResponse( authorizationTokenResponse ) ) );
			  WireMock.stubFor( InitiateUploadTargetRequest( authorizationTokenResponse ).willReturn( SuccessfulInitiateUploadTargetResponse( signedURIPath ) ) );
			  WireMock.stubFor( InitiateUploadRequest( signedURIPath ).willReturn( aResponse().withStatus(HTTP_INTERNAL_ERROR) ) );

			  // when
			  AssertThrows( typeof( CommandFailed ), allOf( containsString( "Unexpected response" ), containsString( "Initiating database upload" ) ), () => authenticateAndCopy(copier, source, "user", "pass".ToCharArray()) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleUploadInACoupleOfRounds() throws java.io.IOException, org.neo4j.commandline.admin.CommandFailed
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleUploadInACoupleOfRounds()
		 {
			  ControlledProgressListener progressListener = new ControlledProgressListener();
			  HttpCopier copier = new HttpCopier(new ControlledOutsideWorld(_fs), millis =>
			  {
			  }, ( name, length ) => progressListener);
			  Path source = CreateDump();
			  long sourceLength = _fs.getFileSize( source.toFile() );
			  long firstUploadLength = sourceLength / 3;
			  string authorizationTokenResponse = "abc";
			  string signedURIPath = "/signed";
			  string uploadLocationPath = "/upload";
			  WireMock.stubFor( AuthenticationRequest( false ).willReturn( SuccessfulAuthorizationResponse( authorizationTokenResponse ) ) );
			  WireMock.stubFor( InitiateUploadTargetRequest( authorizationTokenResponse ).willReturn( SuccessfulInitiateUploadTargetResponse( signedURIPath ) ) );
			  WireMock.stubFor( InitiateUploadRequest( signedURIPath ).willReturn( SuccessfulInitiateUploadResponse( uploadLocationPath ) ) );
			  WireMock.stubFor( ResumeUploadRequest( uploadLocationPath, 0, sourceLength ).willReturn( aResponse().withStatus(HTTP_INTERNAL_ERROR) ) );
			  WireMock.stubFor( GetResumablePositionRequest( sourceLength, uploadLocationPath ).willReturn( UploadIncompleteGetResumablePositionResponse( firstUploadLength ) ) );
			  WireMock.stubFor( ResumeUploadRequest( uploadLocationPath, firstUploadLength, sourceLength ).willReturn( SuccessfulResumeUploadResponse() ) );
			  WireMock.stubFor( TriggerImportRequest( authorizationTokenResponse ).willReturn( SuccessfulTriggerImportResponse() ) );
			  WireMock.stubFor( FirstStatusPollingRequest( authorizationTokenResponse ) );
			  WireMock.stubFor( SecondStatusPollingRequest( authorizationTokenResponse ) );

			  // when
			  AuthenticateAndCopy( copier, source, "user", "pass".ToCharArray() );

			  // then
			  verify( putRequestedFor( urlEqualTo( uploadLocationPath ) ).withHeader( "Content-Length", equalTo( Convert.ToString( sourceLength ) ) ).withoutHeader( "Content-Range" ) );
			  verify( putRequestedFor( urlEqualTo( uploadLocationPath ) ).withHeader( "Content-Length", equalTo( Convert.ToString( sourceLength - firstUploadLength ) ) ).withHeader( "Content-Range", equalTo( format( "bytes %d-%d/%d", firstUploadLength, sourceLength - 1, sourceLength ) ) ) );
			  verify( putRequestedFor( urlEqualTo( uploadLocationPath ) ).withHeader( "Content-Length", equalTo( "0" ) ).withHeader( "Content-Range", equalTo( "bytes */" + sourceLength ) ) );
			  assertTrue( progressListener.DoneCalled );
			  // we need to add 3 to the progress listener because of the database phases
			  assertEquals( sourceLength + 3, progressListener.Progress );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleIncompleteUploadButPositionSaysComplete() throws java.io.IOException, org.neo4j.commandline.admin.CommandFailed
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleIncompleteUploadButPositionSaysComplete()
		 {
			  HttpCopier copier = new HttpCopier(new ControlledOutsideWorld(_fs), millis =>
			  {
			  }, _noOpProgress);
			  Path source = CreateDump();
			  long sourceLength = _fs.getFileSize( source.toFile() );
			  string authorizationTokenResponse = "abc";
			  string signedURIPath = "/signed";
			  string uploadLocationPath = "/upload";
			  WireMock.stubFor( AuthenticationRequest( false ).willReturn( SuccessfulAuthorizationResponse( authorizationTokenResponse ) ) );
			  WireMock.stubFor( InitiateUploadTargetRequest( authorizationTokenResponse ).willReturn( SuccessfulInitiateUploadTargetResponse( signedURIPath ) ) );
			  WireMock.stubFor( InitiateUploadRequest( signedURIPath ).willReturn( SuccessfulInitiateUploadResponse( uploadLocationPath ) ) );
			  WireMock.stubFor( ResumeUploadRequest( uploadLocationPath, 0, sourceLength ).willReturn( aResponse().withStatus(HTTP_INTERNAL_ERROR) ) );
			  WireMock.stubFor( GetResumablePositionRequest( sourceLength, uploadLocationPath ).willReturn( UploadCompleteGetResumablePositionResponse() ) );
			  WireMock.stubFor( TriggerImportRequest( authorizationTokenResponse ).willReturn( SuccessfulTriggerImportResponse() ) );
			  WireMock.stubFor( FirstStatusPollingRequest( authorizationTokenResponse ) );
			  WireMock.stubFor( SecondStatusPollingRequest( authorizationTokenResponse ) );

			  // when
			  AuthenticateAndCopy( copier, source, "user", "pass".ToCharArray() );

			  // then
			  verify( putRequestedFor( urlEqualTo( uploadLocationPath ) ).withHeader( "Content-Length", equalTo( Convert.ToString( sourceLength ) ) ).withoutHeader( "Content-Range" ) );
			  verify( putRequestedFor( urlEqualTo( uploadLocationPath ) ).withHeader( "Content-Length", equalTo( "0" ) ).withHeader( "Content-Range", equalTo( "bytes */" + sourceLength ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleConflictOnTriggerImportAfterUpload() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleConflictOnTriggerImportAfterUpload()
		 {
			  // given
			  HttpCopier copier = new HttpCopier( new ControlledOutsideWorld( _fs ) );
			  Path source = CreateDump();
			  long sourceLength = _fs.getFileSize( source.toFile() );
			  string authorizationTokenResponse = "abc";
			  string signedURIPath = "/signed";
			  string uploadLocationPath = "/upload";
			  WireMock.stubFor( AuthenticationRequest( false ).willReturn( SuccessfulAuthorizationResponse( authorizationTokenResponse ) ) );
			  WireMock.stubFor( InitiateUploadTargetRequest( authorizationTokenResponse ).willReturn( SuccessfulInitiateUploadTargetResponse( signedURIPath ) ) );
			  WireMock.stubFor( InitiateUploadRequest( signedURIPath ).willReturn( SuccessfulInitiateUploadResponse( uploadLocationPath ) ) );
			  WireMock.stubFor( ResumeUploadRequest( uploadLocationPath, sourceLength ).willReturn( SuccessfulResumeUploadResponse() ) );
			  WireMock.stubFor( TriggerImportRequest( authorizationTokenResponse ).willReturn( aResponse().withStatus(HTTP_CONFLICT) ) );

			  // when
			  AssertThrows( typeof( CommandFailed ), containsString( "A non-empty database already exists" ), () => authenticateAndCopy(copier, source, "user", "pass".ToCharArray()) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBackoffAndFailIfTooManyAttempts() throws java.io.IOException, InterruptedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBackoffAndFailIfTooManyAttempts()
		 {
			  // given
			  HttpCopier.Sleeper sleeper = mock( typeof( HttpCopier.Sleeper ) );
			  HttpCopier copier = new HttpCopier( new ControlledOutsideWorld( _fs ), sleeper, _noOpProgress );
			  Path source = CreateDump();
			  long sourceLength = _fs.getFileSize( source.toFile() );
			  string authorizationTokenResponse = "abc";
			  string signedURIPath = "/signed";
			  string uploadLocationPath = "/upload";
			  WireMock.stubFor( AuthenticationRequest( false ).willReturn( SuccessfulAuthorizationResponse( authorizationTokenResponse ) ) );
			  WireMock.stubFor( InitiateUploadTargetRequest( authorizationTokenResponse ).willReturn( SuccessfulInitiateUploadTargetResponse( signedURIPath ) ) );
			  WireMock.stubFor( InitiateUploadRequest( signedURIPath ).willReturn( SuccessfulInitiateUploadResponse( uploadLocationPath ) ) );
			  WireMock.stubFor( ResumeUploadRequest( uploadLocationPath, sourceLength ).willReturn( aResponse().withStatus(HTTP_INTERNAL_ERROR) ) );
			  WireMock.stubFor( GetResumablePositionRequest( sourceLength, uploadLocationPath ).willReturn( UploadIncompleteGetResumablePositionResponse( 0 ) ) );

			  // when/then
			  AssertThrows( typeof( CommandFailed ), containsString( "Upload failed after numerous attempts" ), () => authenticateAndCopy(copier, source, "user", "pass".ToCharArray()) );
			  Mockito.verify( sleeper, atLeast( 30 ) ).sleep( anyLong() );
		 }

		 private MappingBuilder AuthenticationRequest( bool userConsent )
		 {
			  return post( urlEqualTo( "/import/auth" ) ).withHeader( "Authorization", matching( "^Basic .*" ) ).withHeader( "Accept", equalTo( "application/json" ) ).withHeader( "Confirmed", equalTo( userConsent ? "true" : "false" ) );
		 }

		 private ResponseDefinitionBuilder SuccessfulAuthorizationResponse( string authorizationTokenResponse )
		 {
			  return aResponse().withStatus(HTTP_OK).withBody(format("{\"Token\":\"%s\"}", authorizationTokenResponse));
		 }

		 private MappingBuilder InitiateUploadTargetRequest( string authorizationTokenResponse )
		 {
			  return post( urlEqualTo( "/import" ) ).withHeader( "Content-Type", equalTo( "application/json" ) ).withHeader( "Authorization", equalTo( "Bearer " + authorizationTokenResponse ) ).withHeader( "Accept", equalTo( "application/json" ) );
		 }

		 private ResponseDefinitionBuilder SuccessfulInitiateUploadTargetResponse( string signedURIPath )
		 {
			  return aResponse().withStatus(HTTP_ACCEPTED).withBody(format("{\"SignedURI\":\"%s\", \"expiration_date\":\"Fri, 04 Oct 2019 08:21:59 GMT\"}", _testConsoleUrl + signedURIPath));
		 }

		 private MappingBuilder InitiateUploadRequest( string signedURIPath )
		 {
			  return post( urlEqualTo( signedURIPath ) ).withHeader( "Content-Length", equalTo( "0" ) ).withHeader( "x-goog-resumable", equalTo( "start" ) );
		 }

		 private ResponseDefinitionBuilder SuccessfulInitiateUploadResponse( string uploadLocationPath )
		 {
			  return aResponse().withStatus(HTTP_CREATED).withHeader("Location", _testConsoleUrl + uploadLocationPath);
		 }

		 private MappingBuilder ResumeUploadRequest( string uploadLocationPath, long length )
		 {
			  return ResumeUploadRequest( uploadLocationPath, 0, length );
		 }

		 private MappingBuilder ResumeUploadRequest( string uploadLocationPath, long position, long length )
		 {
			  MappingBuilder builder = put( urlEqualTo( uploadLocationPath ) ).withHeader( "Content-Length", equalTo( Convert.ToString( length - position ) ) );
			  if ( position > 0 )
			  {
					builder = builder.withHeader( "Content-Range", equalTo( format( "bytes %d-%d/%d", position, length - 1, length ) ) );
			  }
			  return builder;
		 }

		 private ResponseDefinitionBuilder SuccessfulResumeUploadResponse()
		 {
			  return aResponse().withStatus(HTTP_OK);
		 }

		 private MappingBuilder FirstStatusPollingRequest( string authorizationTokenResponse )
		 {
			  return get( urlEqualTo( "/import/status" ) ).withHeader( "Authorization", equalTo( "Bearer " + authorizationTokenResponse ) ).willReturn( FirstSuccessfulDatabaseRunningResponse() ).inScenario("test").whenScenarioStateIs(Scenario.STARTED).willSetStateTo(STATUS_POLLING_PASSED_FIRST_CALL);
		 }

		 private ResponseDefinitionBuilder FirstSuccessfulDatabaseRunningResponse()
		 {
			  return aResponse().withBody("{\"Status\":\"loading\"}").withStatus(HTTP_OK);
		 }

		 private MappingBuilder SecondStatusPollingRequest( string authorizationTokenResponse )
		 {
			  return get( urlEqualTo( "/import/status" ) ).withHeader( "Authorization", equalTo( "Bearer " + authorizationTokenResponse ) ).willReturn( SecondSuccessfulDatabaseRunningResponse() ).inScenario("test").whenScenarioStateIs(STATUS_POLLING_PASSED_FIRST_CALL);
		 }

		 private ResponseDefinitionBuilder SecondSuccessfulDatabaseRunningResponse()
		 {
			  return aResponse().withBody("{\"Status\":\"running\"}").withStatus(HTTP_OK);
		 }

		 private MappingBuilder TriggerImportRequest( string authorizationTokenResponse )
		 {
			  return post( urlEqualTo( "/import/upload-complete" ) ).withHeader( "Content-Type", equalTo( "application/json" ) ).withHeader( "Authorization", equalTo( "Bearer " + authorizationTokenResponse ) ).withRequestBody( containing( "Crc32" ) );
		 }

		 private ResponseDefinitionBuilder SuccessfulTriggerImportResponse()
		 {
			  return aResponse().withStatus(HTTP_OK);
		 }

		 private ResponseDefinitionBuilder UploadIncompleteGetResumablePositionResponse( long bytesUploadedSoFar )
		 {
			  return aResponse().withStatus(HTTP_RESUME_INCOMPLETE).withHeader("Range", "bytes=0-" + (bytesUploadedSoFar - 1));
		 }

		 private ResponseDefinitionBuilder UploadCompleteGetResumablePositionResponse()
		 {
			  return aResponse().withStatus(HTTP_CREATED);
		 }

		 private MappingBuilder GetResumablePositionRequest( long sourceLength, string uploadLocationPath )
		 {
			  return put( urlEqualTo( uploadLocationPath ) ).withHeader( "Content-Length", equalTo( "0" ) ).withHeader( "Content-Range", equalTo( "bytes */" + sourceLength ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.nio.file.Path createDump() throws java.io.IOException
		 private Path CreateDump()
		 {
			  File file = Directory.file( "something" );
			  assertTrue( file.createNewFile() );
			  Files.write( file.toPath(), "this is simply some weird dump data, but may do the trick for this test of uploading it".GetBytes() );
			  return file.toPath();
		 }

		 private static void AssertThrows( Type exceptionClass, Matcher<string> message, ThrowingRunnable action )
		 {
			  try
			  {
					action.Run();
					fail( "Should have failed" );
			  }
			  catch ( Exception e )
			  {
					assertTrue( exceptionClass.IsInstanceOfType( e ) );
					assertThat( e.Message, message );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void authenticateAndCopy(PushToCloudCommand.Copier copier, java.nio.file.Path source, String username, char[] password) throws org.neo4j.commandline.admin.CommandFailed
		 private void AuthenticateAndCopy( PushToCloudCommand.Copier copier, Path source, string username, char[] password )
		 {
			  string bearerToken = copier.Authenticate( false, _testConsoleUrl, username, password, false );
			  copier.Copy( false, _testConsoleUrl, source, bearerToken );
		 }

		 private interface ThrowingRunnable
		 {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void run() throws Exception;
			  void Run();
		 }

		 private class ControlledProgressListener : ProgressListener
		 {
			  internal long Progress;
			  internal bool DoneCalled;

			  public override void Started( string task )
			  {
			  }

			  public override void Started()
			  {
			  }

			  public override void Set( long progress )
			  {
					throw new System.NotSupportedException( "Should not be called" );
			  }

			  public override void Add( long progress )
			  {
					this.Progress += progress;
			  }

			  public override void Done()
			  {
					DoneCalled = true;
			  }

			  public override void Failed( Exception e )
			  {
					throw new System.NotSupportedException( "Should not be called" );
			  }
		 }
	}

}