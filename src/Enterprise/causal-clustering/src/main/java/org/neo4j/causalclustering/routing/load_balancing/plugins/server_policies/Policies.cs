using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
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
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Neo4Net.causalclustering.routing.load_balancing.plugins.server_policies
{

	using Neo4Net.causalclustering.routing.load_balancing.filters;
	using ProcedureException = Neo4Net.@internal.Kernel.Api.exceptions.ProcedureException;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;
	using Log = Neo4Net.Logging.Log;

	public class Policies
	{
		 public const string POLICY_KEY = "policy";
		 internal const string DEFAULT_POLICY_NAME = "default";
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 internal static readonly Policy DefaultPolicyConflict = new FilteringPolicy( IdentityFilter.@as() ); // the default default

		 private readonly IDictionary<string, Policy> _policies = new Dictionary<string, Policy>();

		 private readonly Log _log;

		 internal Policies( Log log )
		 {
			  this._log = log;
		 }

		 internal virtual void AddPolicy( string policyName, Policy policy )
		 {
			  Policy oldPolicy = _policies.putIfAbsent( policyName, policy );
			  if ( oldPolicy != null )
			  {
					_log.error( format( "Policy name conflict for '%s'.", policyName ) );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: Policy selectFor(java.util.Map<String,String> context) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
		 internal virtual Policy SelectFor( IDictionary<string, string> context )
		 {
			  string policyName = context[POLICY_KEY];

			  if ( string.ReferenceEquals( policyName, null ) )
			  {
					return DefaultPolicy();
			  }
			  else
			  {
					Policy selectedPolicy = _policies[policyName];
					if ( selectedPolicy == null )
					{
						 throw new ProcedureException( Neo4Net.Kernel.Api.Exceptions.Status_Procedure.ProcedureCallFailed, format( "Policy definition for '%s' could not be found.", policyName ) );
					}
					return selectedPolicy;
			  }
		 }

		 private Policy DefaultPolicy()
		 {
			  Policy registeredDefault = _policies[DEFAULT_POLICY_NAME];
			  return registeredDefault != null ? registeredDefault : DefaultPolicyConflict;
		 }
	}

}