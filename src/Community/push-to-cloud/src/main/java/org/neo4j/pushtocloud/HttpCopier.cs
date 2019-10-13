using System;
using System.IO;
using System.Text;
using System.Threading;

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
	using IOUtils = org.apache.commons.compress.utils.IOUtils;
	using JsonIgnoreProperties = org.codehaus.jackson.annotate.JsonIgnoreProperties;
	using ObjectMapper = org.codehaus.jackson.map.ObjectMapper;


	using CommandFailed = Neo4Net.CommandLine.Admin.CommandFailed;
	using OutsideWorld = Neo4Net.CommandLine.Admin.OutsideWorld;
	using ProgressListener = Neo4Net.Helpers.progress.ProgressListener;
	using ProgressMonitorFactory = Neo4Net.Helpers.progress.ProgressMonitorFactory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Long.min;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.apache.commons.compress.utils.IOUtils.toByteArray;

	public class HttpCopier : PushToCloudCommand.Copier
	{
		 internal const int HTTP_RESUME_INCOMPLETE = 308;
		 private const long POSITION_UPLOAD_COMPLETED = -1;
		 private static readonly long _maximumRetryBackoff = SECONDS.toMillis( 64 );

		 private readonly OutsideWorld _outsideWorld;
		 private readonly Sleeper _sleeper;
		 private readonly ProgressListenerFactory _progressListenerFactory;

		 internal HttpCopier( OutsideWorld outsideWorld ) : this( outsideWorld, Thread.sleep, ( text, length ) -> ProgressMonitorFactory.textual( outsideWorld.OutStream() ).singlePart(text, length) )
		 {
		 }

		 internal HttpCopier( OutsideWorld outsideWorld, Sleeper sleeper, ProgressListenerFactory progressListenerFactory )
		 {
			  this._outsideWorld = outsideWorld;
			  this._sleeper = sleeper;
			  this._progressListenerFactory = progressListenerFactory;
		 }

		 /// <summary>
		 /// Do the actual transfer of the source (a Neo4j database dump) to the target.
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void copy(boolean verbose, String consoleURL, java.nio.file.Path source, String bearerToken) throws org.neo4j.commandline.admin.CommandFailed
		 public override void Copy( bool verbose, string consoleURL, Path source, string bearerToken )
		 {
			  try
			  {
					string bearerTokenHeader = "Bearer " + bearerToken;
					long crc32Sum = CalculateCrc32HashOfFile( source );
					URL signedURL = InitiateCopy( verbose, SafeUrl( consoleURL + "/import" ), crc32Sum, bearerTokenHeader );
					URL uploadLocation = InitiateResumableUpload( verbose, signedURL );
					long sourceLength = _outsideWorld.fileSystem().getFileSize(source.toFile());

					// Enter the resume:able upload loop
					long position = 0;
					int retries = 0;
					ThreadLocalRandom random = ThreadLocalRandom.current();
					ProgressTrackingOutputStream.Progress uploadProgress = new ProgressTrackingOutputStream.Progress( _progressListenerFactory.create( "Upload", sourceLength ), position );
					while ( !ResumeUpload( verbose, source, sourceLength, position, uploadLocation, uploadProgress ) )
					{
						 position = GetResumablePosition( verbose, sourceLength, uploadLocation );
						 if ( position == POSITION_UPLOAD_COMPLETED )
						 {
							  // This is somewhat unexpected, we didn't get an OK from the upload, but when we asked about how far the upload
							  // got it responded that it was fully uploaded. I'd guess we're fine here.
							  break;
						 }

						 // Truncated exponential backoff
						 if ( retries > 50 )
						 {
							  throw new CommandFailed( "Upload failed after numerous attempts. The upload can be resumed with this command: TODO" );
						 }
						 long backoffFromRetryCount = SECONDS.toMillis( 1 << retries++ ) + random.Next( 1_000 );
						 _sleeper.sleep( min( backoffFromRetryCount, _maximumRetryBackoff ) );
					}
					uploadProgress.Done();

					TriggerImportProtocol( verbose, SafeUrl( consoleURL + "/import/upload-complete" ), crc32Sum, bearerTokenHeader );

					DoStatusPolling( verbose, consoleURL, bearerToken );
			  }
			  catch ( Exception e ) when ( e is InterruptedException || e is IOException )
			  {
					throw new CommandFailed( e.Message, e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void doStatusPolling(boolean verbose, String consoleURL, String bearerToken) throws java.io.IOException, InterruptedException, org.neo4j.commandline.admin.CommandFailed
		 private void DoStatusPolling( bool verbose, string consoleURL, string bearerToken )
		 {
			  _outsideWorld.stdOutLine( "We have received your export and it is currently being loaded into your cloud instance." );
			  _outsideWorld.stdOutLine( "You can wait here, or abort this command and head over to the console to be notified of when your database is running." );
			  string bearerTokenHeader = "Bearer " + bearerToken;
			  ProgressTrackingOutputStream.Progress statusProgress = new ProgressTrackingOutputStream.Progress( _progressListenerFactory.create( "Import status", 3L ), 0 );
			  bool firstRunning = true;
			  long importStartedTimeout = DateTimeHelper.CurrentUnixTimeMillis() + 90 * 1000; // timeout to switch from first running to loading = 1.5 minute
			  while ( !statusProgress.Done )
			  {
					string status = GetDatabaseStatus( verbose, SafeUrl( consoleURL + "/import/status" ), bearerTokenHeader );
					switch ( status )
					{
						 case "running":
							  // It could happen that the very first call of this method is so fast, that the database is still in state
							  // "running". So we need to check if this is the case and ignore the result in that case and only
							  // take this result as valid, once the status loading or restoring was seen before.
							  if ( !firstRunning )
							  {
									statusProgress.RewindTo( 0 );
									statusProgress.Add( 3 );
									statusProgress.Done();
							  }
							  else
							  {
									bool passedStartImportTimeout = DateTimeHelper.CurrentUnixTimeMillis() > importStartedTimeout;
									if ( passedStartImportTimeout )
									{
										 throw new CommandFailed( "We're sorry, it couldn't be detected that the import was started, " + "please check the console for further details." );
									}
							  }
							  break;
						 case "loading":
							  firstRunning = false;
							  statusProgress.RewindTo( 0 );
							  statusProgress.Add( 1 );
							  break;
						 case "restoring":
							  firstRunning = false;
							  statusProgress.RewindTo( 0 );
							  statusProgress.Add( 2 );
							  break;
						 case "loading failed":
							  throw new CommandFailed( "We're sorry, something has gone wrong. We did not recognize the file you uploaded as a valid Neo4j dump file. " + "Please check the file and try again. If you have received this error after confirming the type of file being uploaded," + "please open a support case." );
						 default:
							  throw new CommandFailed( string.Format( "We're sorry, something has failed during the loading of your database. " + "Please try again and if this problem persists, please open up a support case. Database status: {0}", status ) );
					}
					_sleeper.sleep( 2000 );
			  }
			  _outsideWorld.stdOutLine( "Your data was successfully pushed to cloud and is now running." );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public String authenticate(boolean verbose, String consoleUrl, String username, char[] password, boolean consentConfirmed) throws org.neo4j.commandline.admin.CommandFailed
		 public override string Authenticate( bool verbose, string consoleUrl, string username, char[] password, bool consentConfirmed )
		 {
			  try
			  {
					URL url = SafeUrl( consoleUrl + "/import/auth" );
					HttpURLConnection connection = ( HttpURLConnection ) url.openConnection();
					try
					{
						 connection.RequestMethod = "POST";
						 connection.setRequestProperty( "Authorization", "Basic " + Base64Encode( username, password ) );
						 connection.setRequestProperty( "Accept", "application/json" );
						 connection.setRequestProperty( "Confirmed", consentConfirmed.ToString() );
						 int responseCode = connection.ResponseCode;
						 switch ( responseCode )
						 {
						 case HTTP_NOT_FOUND:
							  // fallthrough
						 case HTTP_MOVED_PERM:
							  throw UpdatePluginErrorResponse( connection );
						 case HTTP_UNAUTHORIZED:
							  throw ErrorResponse( verbose, connection, "Invalid username/password credentials" );
						 case HTTP_FORBIDDEN:
							  throw ErrorResponse( verbose, connection, "The given credentials do not give administrative access to the target database" );
						 case HTTP_CONFLICT:
							  // the cloud target database has already been populated with data, and importing the dump file would overwrite it.
							  bool consent = AskForBooleanConsent( "A non-empty database already exists at the given location, would you like to overwrite that database?" );
							  if ( consent )
							  {
									return Authenticate( verbose, consoleUrl, username, password, true );
							  }
							  else
							  {
									throw ErrorResponse( verbose, connection, "No consent to overwrite database, aborting upload" );
							  }
						 case HTTP_OK:
							  using ( Stream responseData = connection.InputStream )
							  {
									string json = new string( toByteArray( responseData ), UTF_8 );
									return ParseJsonUsingJacksonParser( json, typeof( TokenBody ) ).Token;
							  }
						 default:
							  throw UnexpectedResponse( verbose, connection, "Authorization" );
						 }
					}
					finally
					{
						 connection.disconnect();
					}
			  }
			  catch ( IOException e )
			  {
					throw new CommandFailed( e.Message, e );
			  }
		 }

		 /// <summary>
		 /// Communication with Neo4j's cloud console, resulting in some signed URI to do the actual upload to.
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.net.URL initiateCopy(boolean verbose, java.net.URL importURL, long crc32Sum, String bearerToken) throws java.io.IOException, org.neo4j.commandline.admin.CommandFailed
		 private URL InitiateCopy( bool verbose, URL importURL, long crc32Sum, string bearerToken )
		 {
			  HttpURLConnection connection = ( HttpURLConnection ) importURL.openConnection();
			  try
			  {
					// POST the request
					connection.RequestMethod = "POST";
					connection.setRequestProperty( "Content-Type", "application/json" );
					connection.setRequestProperty( "Authorization", bearerToken );
					connection.setRequestProperty( "Accept", "application/json" );
					connection.DoOutput = true;
					using ( Stream postData = connection.OutputStream )
					{
						 postData.WriteByte( BuildCrc32WithConsentJson( crc32Sum ).GetBytes( UTF_8 ) );
					}

					// Read the response
					int responseCode = connection.ResponseCode;
					switch ( responseCode )
					{
					case HTTP_NOT_FOUND:
						 // fallthrough
					case HTTP_MOVED_PERM:
						 throw UpdatePluginErrorResponse( connection );
					case HTTP_UNAUTHORIZED:
						 throw ErrorResponse( verbose, connection, "The given authorization token is invalid or has expired" );
					case HTTP_ACCEPTED:
						 // the import request was accepted, and the server has not seen this dump file, meaning the import request is a new operation.
						 return SafeUrl( ExtractSignedURIFromResponse( verbose, connection ) );
					default:
						 throw UnexpectedResponse( verbose, connection, "Initiating upload target" );
					}
			  }
			  finally
			  {
					connection.disconnect();
			  }
		 }

		 /// <summary>
		 /// Makes initial contact with the signed URL we got back when talking to the Neo4j cloud console. This will create yet another URL
		 /// which will be used to upload the source to, potentially resumed if it gets interrupted in the middle.
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.net.URL initiateResumableUpload(boolean verbose, java.net.URL signedURL) throws java.io.IOException, org.neo4j.commandline.admin.CommandFailed
		 private URL InitiateResumableUpload( bool verbose, URL signedURL )
		 {
			  HttpURLConnection connection = ( HttpURLConnection ) signedURL.openConnection();
			  try
			  {
					connection.RequestMethod = "POST";
					connection.setRequestProperty( "Content-Length", "0" );
					connection.FixedLengthStreamingMode = 0;
					connection.setRequestProperty( "x-goog-resumable", "start" );
					// We don't want to have any Content-Type set really, but there's this issue with the standard HttpURLConnection
					// implementation where it defaults Content-Type to application/x-www-form-urlencoded for POSTs for some reason
					connection.setRequestProperty( "Content-Type", "" );
					connection.DoOutput = true;
					int responseCode = connection.ResponseCode;
					if ( responseCode != HTTP_CREATED )
					{
						 throw UnexpectedResponse( verbose, connection, "Initiating database upload" );
					}
					return SafeUrl( connection.getHeaderField( "Location" ) );
			  }
			  finally
			  {
					connection.disconnect();
			  }
		 }

		 /// <summary>
		 /// Uploads source from the given position to the upload location.
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private boolean resumeUpload(boolean verbose, java.nio.file.Path source, long sourceLength, long position, java.net.URL uploadLocation, ProgressTrackingOutputStream.Progress uploadProgress) throws java.io.IOException, org.neo4j.commandline.admin.CommandFailed
		 private bool ResumeUpload( bool verbose, Path source, long sourceLength, long position, URL uploadLocation, ProgressTrackingOutputStream.Progress uploadProgress )
		 {
			  HttpURLConnection connection = ( HttpURLConnection ) uploadLocation.openConnection();
			  try
			  {
					connection.RequestMethod = "PUT";
					long contentLength = sourceLength - position;
					connection.setRequestProperty( "Content-Length", contentLength.ToString() );
					connection.FixedLengthStreamingMode = contentLength;
					if ( position > 0 )
					{
						 // If we're not starting from the beginning we need to specify what range we're uploading in this format
						 connection.setRequestProperty( "Content-Range", format( "bytes %d-%d/%d", position, sourceLength - 1, sourceLength ) );
					}
					connection.DoOutput = true;
					uploadProgress.RewindTo( position );
					using ( Stream sourceStream = new FileStream( source.toFile(), FileMode.Open, FileAccess.Read ), Stream targetStream = connection.OutputStream )
					{
						 SafeSkip( sourceStream, position );
						 IOUtils.copy( new BufferedInputStream( sourceStream ), new ProgressTrackingOutputStream( targetStream, uploadProgress ) );
					}
					int responseCode = connection.ResponseCode;
					switch ( responseCode )
					{
					case HTTP_OK:
						 return true; // the file is now uploaded, all good
					case HTTP_INTERNAL_ERROR:
					case HTTP_UNAVAILABLE:
						 DebugErrorResponse( verbose, connection );
						 return false;
					default:
						 throw UnexpectedResponse( verbose, connection, "Resumable database upload" );
					}
			  }
			  finally
			  {
					connection.disconnect();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void triggerImportProtocol(boolean verbose, java.net.URL importURL, long crc32Sum, String bearerToken) throws java.io.IOException, org.neo4j.commandline.admin.CommandFailed
		 private void TriggerImportProtocol( bool verbose, URL importURL, long crc32Sum, string bearerToken )
		 {
			  HttpURLConnection connection = ( HttpURLConnection ) importURL.openConnection();
			  try
			  {
					connection.RequestMethod = "POST";
					connection.setRequestProperty( "Content-Type", "application/json" );
					connection.setRequestProperty( "Authorization", bearerToken );
					connection.DoOutput = true;
					using ( Stream postData = connection.OutputStream )
					{
						 postData.WriteByte( BuildCrc32WithConsentJson( crc32Sum ).GetBytes( UTF_8 ) );
					}

					int responseCode = connection.ResponseCode;
					switch ( responseCode )
					{
					case HTTP_NOT_FOUND:
						 // fallthrough
					case HTTP_MOVED_PERM:
						 throw UpdatePluginErrorResponse( connection );
					case HTTP_CONFLICT:
						 throw ErrorResponse( verbose, connection, "A non-empty database already exists at the given location and overwrite consent not given, aborting" );
					case HTTP_OK:
						 // All good, we managed to trigger the import protocol after our completed upload
						 break;
					default:
						 throw UnexpectedResponse( verbose, connection, "Trigger import/restore after successful upload" );
					}
			  }
			  finally
			  {
					connection.disconnect();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private String getDatabaseStatus(boolean verbose, java.net.URL statusURL, String bearerToken) throws java.io.IOException, org.neo4j.commandline.admin.CommandFailed
		 private string GetDatabaseStatus( bool verbose, URL statusURL, string bearerToken )
		 {
			  HttpURLConnection connection = ( HttpURLConnection ) statusURL.openConnection();
			  try
			  {
					connection.RequestMethod = "GET";
					connection.setRequestProperty( "Authorization", bearerToken );
					connection.DoOutput = true;

					int responseCode = connection.ResponseCode;
					switch ( responseCode )
					{
						 case HTTP_NOT_FOUND:
							  // fallthrough
						 case HTTP_MOVED_PERM:
							  throw UpdatePluginErrorResponse( connection );
						 case HTTP_OK:
							  using ( Stream responseData = connection.InputStream )
							  {
									string json = new string( toByteArray( responseData ), UTF_8 );
									return ParseJsonUsingJacksonParser( json, typeof( StatusBody ) ).Status;
							  }
						 default:
							  throw UnexpectedResponse( verbose, connection, "Trigger import/restore after successful upload" );
					}
			  }
			  finally
			  {
					connection.disconnect();
			  }
		 }

		 /// <summary>
		 /// Asks about how far the upload has gone so far, typically after being interrupted one way or another. The result of this method
		 /// can be fed into <seealso cref="resumeUpload(bool, Path, long, long, URL, ProgressTrackingOutputStream.Progress)"/> to resume an upload.
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private long getResumablePosition(boolean verbose, long sourceLength, java.net.URL uploadLocation) throws java.io.IOException, org.neo4j.commandline.admin.CommandFailed
		 private long GetResumablePosition( bool verbose, long sourceLength, URL uploadLocation )
		 {
			  Debug( verbose, "Asking about resumable position for the upload" );
			  HttpURLConnection connection = ( HttpURLConnection ) uploadLocation.openConnection();
			  try
			  {
					connection.RequestMethod = "PUT";
					connection.setRequestProperty( "Content-Length", "0" );
					connection.FixedLengthStreamingMode = 0;
					connection.setRequestProperty( "Content-Range", "bytes */" + sourceLength );
					connection.DoOutput = true;
					int responseCode = connection.ResponseCode;
					switch ( responseCode )
					{
					case HTTP_OK:
					case HTTP_CREATED:
						 Debug( verbose, "Upload seems to be completed got " + responseCode );
						 return POSITION_UPLOAD_COMPLETED;
					case HTTP_RESUME_INCOMPLETE:
						 string range = connection.getHeaderField( "Range" );
						 Debug( verbose, "Upload not completed got " + range );
						 long position = string.ReferenceEquals( range, null ) ? 0 : ParseResumablePosition( range );
						 Debug( verbose, "Parsed that as position " + position );
						 return position;
					default:
						 throw UnexpectedResponse( verbose, connection, "Acquire resumable upload position" );
					}
			  }
			  finally
			  {
					connection.disconnect();
			  }
		 }

		 private static string BuildCrc32WithConsentJson( long crc32Sum )
		 {
			  return string.Format( "{{\"Crc32\":{0:D}}}", crc32Sum );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void safeSkip(java.io.InputStream sourceStream, long position) throws java.io.IOException
		 private static void SafeSkip( Stream sourceStream, long position )
		 {
			  long toSkip = position;
			  while ( toSkip > 0 )
			  {
					toSkip -= sourceStream.skip( position );
			  }
		 }

		 /// <summary>
		 /// Parses a response from asking about how far an upload has gone, i.e. how many bytes of the source file have been uploaded.
		 /// The range is in the format: "bytes=x-y" and since we always ask from 0 then we're interested in y, more specifically y+1
		 /// since x-y means that bytes in the range x-y have been received so we want to start sending from y+1.
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static long parseResumablePosition(String range) throws org.neo4j.commandline.admin.CommandFailed
		 private static long ParseResumablePosition( string range )
		 {
			  int dashIndex = range.IndexOf( '-' );
			  if ( !range.StartsWith( "bytes=", StringComparison.Ordinal ) || dashIndex == -1 )
			  {
					throw new CommandFailed( "Unexpected response when asking where to resume upload from. got '" + range + "'" );
			  }
			  return long.Parse( range.Substring( dashIndex + 1 ) ) + 1;
		 }

		 private bool AskForBooleanConsent( string message )
		 {
			  while ( true )
			  {
					string input = _outsideWorld.promptLine( message );
					if ( !string.ReferenceEquals( input, null ) )
					{
						 input = input.ToLower();
						 if ( input.Equals( "yes" ) || input.Equals( "y" ) || input.Equals( "true" ) )
						 {
							  return true;
						 }
						 if ( input.Equals( "no" ) || input.Equals( "n" ) || input.Equals( "false" ) )
						 {
							  return false;
						 }
					}
					_outsideWorld.stdOutLine( "Sorry, I didn't understand your answer. Please reply with yes/y or no/n" );
			  }
		 }

		 private static string Base64Encode( string username, char[] password )
		 {
			  string plainToken = ( new StringBuilder( username ) ).Append( ":" ).Append( password ).ToString();
			  return Base64.Encoder.encodeToString( plainToken.GetBytes() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private String extractSignedURIFromResponse(boolean verbose, java.net.HttpURLConnection connection) throws java.io.IOException, org.neo4j.commandline.admin.CommandFailed
		 private string ExtractSignedURIFromResponse( bool verbose, HttpURLConnection connection )
		 {
			  using ( Stream responseData = connection.InputStream )
			  {
					string json = new string( toByteArray( responseData ), UTF_8 );
					Debug( verbose, "Got json '" + json + "' back expecting to contain the signed URL" );
					return ParseJsonUsingJacksonParser( json, typeof( SignedURIBody ) ).SignedURI;
			  }
		 }

		 private void Debug( bool verbose, string @string )
		 {
			  if ( verbose )
			  {
					_outsideWorld.stdOutLine( @string );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void debugErrorResponse(boolean verbose, java.net.HttpURLConnection connection) throws java.io.IOException
		 private void DebugErrorResponse( bool verbose, HttpURLConnection connection )
		 {
			  DebugResponse( verbose, connection, false );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void debugResponse(boolean verbose, java.net.HttpURLConnection connection, boolean successful) throws java.io.IOException
		 private void DebugResponse( bool verbose, HttpURLConnection connection, bool successful )
		 {
			  if ( verbose )
			  {
					Debug( true, "=== Unexpected response ===" );
					Debug( true, "Response message: " + connection.ResponseMessage );
					Debug( true, "Response headers:" );
					connection.HeaderFields.forEach((key, value1) =>
					{
					 foreach ( string value in value1 )
					 {
						  Debug( true, "  " + key + ": " + value );
					 }
					});
					using ( Stream responseData = successful ? connection.InputStream : connection.ErrorStream )
					{
						 string responseString = new string( toByteArray( responseData ), UTF_8 );
						 Debug( true, "Error response data: " + responseString );
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static long calculateCrc32HashOfFile(java.nio.file.Path source) throws java.io.IOException
		 private static long CalculateCrc32HashOfFile( Path source )
		 {
			  CRC32 crc = new CRC32();
			  using ( Stream inputStream = new BufferedInputStream( new FileStream( source.toFile(), FileMode.Open, FileAccess.Read ) ) )
			  {
					int cnt;
					while ( ( cnt = inputStream.Read() ) != -1 )
					{
						 crc.update( cnt );
					}
			  }
			  return crc.Value;
		 }

		 private static URL SafeUrl( string urlString )
		 {
			  try
			  {
					return new URL( urlString );
			  }
			  catch ( MalformedURLException e )
			  {
					throw new Exception( "Malformed URL '" + urlString + "'", e );
			  }
		 }

		 /// <summary>
		 /// Use the Jackson JSON parser because Neo4j Server depends on this library already and therefore already exists in the environment.
		 /// This means that this command can parse JSON w/o any additional external dependency and doesn't even need to depend on java 8,
		 /// where the Rhino script engine has built-in JSON parsing support.
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static <T> T parseJsonUsingJacksonParser(String json, Class<T> type) throws java.io.IOException
		 private static T ParseJsonUsingJacksonParser<T>( string json, Type type )
		 {
				 type = typeof( T );
			  return ( new ObjectMapper() ).readValue(json, type);
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.commandline.admin.CommandFailed errorResponse(boolean verbose, java.net.HttpURLConnection connection, String errorDescription) throws java.io.IOException
		 private CommandFailed ErrorResponse( bool verbose, HttpURLConnection connection, string errorDescription )
		 {
			  DebugErrorResponse( verbose, connection );
			  return new CommandFailed( errorDescription );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.commandline.admin.CommandFailed updatePluginErrorResponse(java.net.HttpURLConnection connection) throws java.io.IOException
		 private CommandFailed UpdatePluginErrorResponse( HttpURLConnection connection )
		 {
			  DebugErrorResponse( true, connection );
			  return new CommandFailed( "We encountered a problem while communicating to the Neo4j cloud system. " + "Please check that you are using the latest version of the push-to-cloud plugin and upgrade if necessary. " + "If this problem persists after upgrading, please contact support and attach the logs shown below to your ticket in the support portal." );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.commandline.admin.CommandFailed unexpectedResponse(boolean verbose, java.net.HttpURLConnection connection, String requestDescription) throws java.io.IOException
		 private CommandFailed UnexpectedResponse( bool verbose, HttpURLConnection connection, string requestDescription )
		 {
			  return ErrorResponse( verbose, connection, format( "Unexpected response code %d from request: %s", connection.ResponseCode, requestDescription ) );
		 }

		 // Simple structs for mapping JSON to objects, used by the jackson parser which Neo4j happens to depend on anyway
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @JsonIgnoreProperties(ignoreUnknown = true) private static class SignedURIBody
		 private class SignedURIBody
		 {
			  public string SignedURI;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @JsonIgnoreProperties(ignoreUnknown = true) private static class TokenBody
		 private class TokenBody
		 {
			  public string Token;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @JsonIgnoreProperties(ignoreUnknown = true) private static class StatusBody
		 private class StatusBody
		 {
			  public string Status;
		 }

		 internal interface Sleeper
		 {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void sleep(long millis) throws InterruptedException;
			  void Sleep( long millis );
		 }

		 public interface ProgressListenerFactory
		 {
			  ProgressListener Create( string text, long length );
		 }
	}

}