using System.Collections.Generic;

/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
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
namespace Neo4Net.Bolt.v1.runtime
{

	using BoltStateMachineSPI = Neo4Net.Bolt.runtime.BoltStateMachineSPI;
	using Neo4NetError = Neo4Net.Bolt.runtime.Neo4NetError;
	using TransactionStateMachineSPI = Neo4Net.Bolt.runtime.TransactionStateMachineSPI;
	using Authentication = Neo4Net.Bolt.security.auth.Authentication;
	using AuthenticationException = Neo4Net.Bolt.security.auth.AuthenticationException;
	using AuthenticationResult = Neo4Net.Bolt.security.auth.AuthenticationResult;
	using Version = Neo4Net.Kernel.Internal.Version;
	using LogService = Neo4Net.Logging.Internal.LogService;
	using UsageData = Neo4Net.Udc.UsageData;
	using UsageDataKeys = Neo4Net.Udc.UsageDataKeys;

	public class BoltStateMachineV1SPI : BoltStateMachineSPI
	{
		 public const string BOLT_SERVER_VERSION_PREFIX = "Neo4Net/";
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
			  this._version = BOLT_SERVER_VERSION_PREFIX + Version.Neo4NetVersion;
		 }

		 public override TransactionStateMachineSPI TransactionSpi()
		 {
			  return _transactionSpi;
		 }

		 public override void ReportError( Neo4NetError err )
		 {
			  _errorReporter.report( err );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Neo4Net.bolt.security.auth.AuthenticationResult authenticate(java.util.Map<String,Object> authToken) throws Neo4Net.bolt.security.auth.AuthenticationException
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