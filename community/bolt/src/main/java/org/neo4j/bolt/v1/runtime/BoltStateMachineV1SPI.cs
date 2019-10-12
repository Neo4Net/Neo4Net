using System.Collections.Generic;

/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace Org.Neo4j.Bolt.v1.runtime
{

	using BoltStateMachineSPI = Org.Neo4j.Bolt.runtime.BoltStateMachineSPI;
	using Neo4jError = Org.Neo4j.Bolt.runtime.Neo4jError;
	using TransactionStateMachineSPI = Org.Neo4j.Bolt.runtime.TransactionStateMachineSPI;
	using Authentication = Org.Neo4j.Bolt.security.auth.Authentication;
	using AuthenticationException = Org.Neo4j.Bolt.security.auth.AuthenticationException;
	using AuthenticationResult = Org.Neo4j.Bolt.security.auth.AuthenticationResult;
	using Version = Org.Neo4j.Kernel.@internal.Version;
	using LogService = Org.Neo4j.Logging.@internal.LogService;
	using UsageData = Org.Neo4j.Udc.UsageData;
	using UsageDataKeys = Org.Neo4j.Udc.UsageDataKeys;

	public class BoltStateMachineV1SPI : BoltStateMachineSPI
	{
		 public const string BOLT_SERVER_VERSION_PREFIX = "Neo4j/";
		 private readonly UsageData _usageData;
		 private readonly ErrorReporter _errorReporter;
		 private readonly Authentication _authentication;
		 private readonly string _version;
		 private readonly TransactionStateMachineSPI _transactionSpi;

		 public BoltStateMachineV1SPI( UsageData usageData, LogService logging, Authentication authentication, TransactionStateMachineSPI transactionStateMachineSPI )
		 {
			  this._usageData = usageData;
			  this._errorReporter = new ErrorReporter( logging );
			  this._authentication = authentication;
			  this._transactionSpi = transactionStateMachineSPI;
			  this._version = BOLT_SERVER_VERSION_PREFIX + Version.Neo4jVersion;
		 }

		 public override TransactionStateMachineSPI TransactionSpi()
		 {
			  return _transactionSpi;
		 }

		 public override void ReportError( Neo4jError err )
		 {
			  _errorReporter.report( err );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.bolt.security.auth.AuthenticationResult authenticate(java.util.Map<String,Object> authToken) throws org.neo4j.bolt.security.auth.AuthenticationException
		 public override AuthenticationResult Authenticate( IDictionary<string, object> authToken )
		 {
			  return _authentication.authenticate( authToken );
		 }

		 public override void UdcRegisterClient( string clientName )
		 {
			  _usageData.get( UsageDataKeys.clientNames ).add( clientName );
		 }

		 public override string Version()
		 {
			  return _version;
		 }
	}

}