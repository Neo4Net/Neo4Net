using System;
using System.Collections.Generic;
using System.Text;

/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.Kernel.impl.query
{

	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using ExecutingQuery = Neo4Net.Kernel.Api.query.ExecutingQuery;
	using QuerySnapshot = Neo4Net.Kernel.Api.query.QuerySnapshot;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Log = Neo4Net.Logging.Log;

	internal class ConfiguredQueryLogger : QueryLogger
	{
		 private readonly Log _log;
		 private readonly long _thresholdMillis;
		 private readonly bool _logQueryParameters;
		 private readonly bool _logDetailedTime;
		 private readonly bool _logAllocatedBytes;
		 private readonly bool _logPageDetails;
		 private readonly bool _logRuntime;

		 private static readonly Pattern _passwordPattern = Pattern.compile( "(?:(?i)call)\\s+dbms(?:\\.security)?\\.change(?:User)?Password\\(" + "(?:\\s*(?:'(?:(?<=\\\\)'|[^'])*'|\"(?:(?<=\\\\)\"|[^\"])*\"|[^,]*)\\s*,)?" + "\\s*('(?:(?<=\\\\)'|[^'])*'|\"(?:(?<=\\\\)\"|[^\"])*\"|\\$\\w*|\\{\\w*})\\s*\\)" );

		 internal ConfiguredQueryLogger( Log log, Config config )
		 {
			  this._log = log;
			  this._thresholdMillis = config.Get( GraphDatabaseSettings.log_queries_threshold ).toMillis();
			  this._logQueryParameters = config.Get( GraphDatabaseSettings.log_queries_parameter_logging_enabled );
			  this._logDetailedTime = config.Get( GraphDatabaseSettings.log_queries_detailed_time_logging_enabled );
			  this._logAllocatedBytes = config.Get( GraphDatabaseSettings.log_queries_allocation_logging_enabled );
			  this._logPageDetails = config.Get( GraphDatabaseSettings.log_queries_page_detail_logging_enabled );
			  this._logRuntime = config.Get( GraphDatabaseSettings.log_queries_runtime_logging_enabled );
		 }

		 public override void Failure( ExecutingQuery query, Exception failure )
		 {
			  _log.error( LogEntry( query.Snapshot() ), failure );
		 }

		 public override void Success( ExecutingQuery query )
		 {
			  if ( NANOSECONDS.toMillis( query.ElapsedNanos() ) >= _thresholdMillis )
			  {
					QuerySnapshot snapshot = query.Snapshot();
					_log.info( LogEntry( snapshot ) );
			  }
		 }

		 private string LogEntry( QuerySnapshot query )
		 {
			  string sourceString = query.ClientConnection().asConnectionDetails();
			  string queryText = query.QueryText();

			  ISet<string> passwordParams = new HashSet<string>();
			  Matcher matcher = _passwordPattern.matcher( queryText );

			  while ( matcher.find() )
			  {
					string password = matcher.group( 1 ).Trim();
					if ( password[0] == '$' )
					{
						 passwordParams.Add( password.Substring( 1 ) );
					}
					else if ( password[0] == '{' )
					{
						 passwordParams.Add( password.Substring( 1, ( password.Length - 1 ) - 1 ) );
					}
					else
					{
						 queryText = queryText.Replace( password, "******" );
					}
			  }

			  StringBuilder result = new StringBuilder();
			  result.Append( TimeUnit.MICROSECONDS.toMillis( query.ElapsedTimeMicros() ) ).Append(" ms: ");
			  if ( _logDetailedTime )
			  {
					QueryLogFormatter.FormatDetailedTime( result, query );
			  }
			  if ( _logAllocatedBytes )
			  {
					QueryLogFormatter.FormatAllocatedBytes( result, query );
			  }
			  if ( _logPageDetails )
			  {
					QueryLogFormatter.FormatPageDetails( result, query );
			  }
			  result.Append( sourceString ).Append( " - " ).Append( queryText );
			  if ( _logQueryParameters )
			  {
					QueryLogFormatter.FormatMapValue( result.Append( " - " ), query.QueryParameters(), passwordParams );
			  }
			  if ( _logRuntime )
			  {
					result.Append( " - runtime=" ).Append( query.Runtime() );
			  }
			  QueryLogFormatter.FormatMap( result.Append( " - " ), query.TransactionAnnotationData() );
			  return result.ToString();
		 }
	}

}