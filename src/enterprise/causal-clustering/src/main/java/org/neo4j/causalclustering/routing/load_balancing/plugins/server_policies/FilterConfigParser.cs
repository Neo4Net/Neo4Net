using System;
using System.Collections.Generic;

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
namespace Neo4Net.causalclustering.routing.load_balancing.plugins.server_policies
{

	using Neo4Net.causalclustering.routing.load_balancing.filters;
	using Neo4Net.causalclustering.routing.load_balancing.filters;
	using Neo4Net.causalclustering.routing.load_balancing.filters;
	using Neo4Net.causalclustering.routing.load_balancing.filters;
	using Neo4Net.causalclustering.routing.load_balancing.filters;


	public class FilterConfigParser
	{
		 private FilterConfigParser()
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static org.Neo4Net.causalclustering.routing.load_balancing.filters.Filter<ServerInfo> filterFor(String filterName, String[] args) throws InvalidFilterSpecification
		 private static Filter<ServerInfo> FilterFor( string filterName, string[] args )
		 {
			  switch ( filterName )
			  {
			  case "groups":
					if ( args.Length < 1 )
					{
						 throw new InvalidFilterSpecification( format( "Invalid number of arguments for filter '%s': %d", filterName, args.Length ) );
					}
					foreach ( string group in args )
					{
						 if ( group.matches( "\\W" ) )
						 {
							  throw new InvalidFilterSpecification( format( "Invalid group for filter '%s': '%s'", filterName, group ) );
						 }
					}
					return new AnyGroupFilter( args );
			  case "min":
					if ( args.Length != 1 )
					{
						 throw new InvalidFilterSpecification( format( "Invalid number of arguments for filter '%s': %d", filterName, args.Length ) );
					}
					int minCount;
					try
					{
						 minCount = int.Parse( args[0] );
					}
					catch ( System.FormatException e )
					{
						 throw new InvalidFilterSpecification( format( "Invalid argument for filter '%s': '%s'", filterName, args[0] ), e );
					}
					return new MinimumCountFilter<ServerInfo>( minCount );
			  case "all":
					if ( args.Length != 0 )
					{
						 throw new InvalidFilterSpecification( format( "Invalid number of arguments for filter '%s': %d", filterName, args.Length ) );
					}
					return IdentityFilter.@as();
			  case "halt":
					if ( args.Length != 0 )
					{
						 throw new InvalidFilterSpecification( format( "Invalid number of arguments for filter '%s': %d", filterName, args.Length ) );
					}
					return HaltFilter.Instance;
			  default:
					throw new InvalidFilterSpecification( "Unknown filter: " + filterName );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static org.Neo4Net.causalclustering.routing.load_balancing.filters.Filter<ServerInfo> parse(String filterConfig) throws InvalidFilterSpecification
		 public static Filter<ServerInfo> Parse( string filterConfig )
		 {
			  if ( filterConfig.Length == 0 )
			  {
					throw new InvalidFilterSpecification( "Filter config is empty" );
			  }

			  IList<FilterChain<ServerInfo>> rules = new List<FilterChain<ServerInfo>>();
			  string[] ruleSpecs = filterConfig.Split( ";", true );

			  if ( ruleSpecs.Length == 0 )
			  {
					throw new InvalidFilterSpecification( "No rules specified" );
			  }

			  bool haltFilterEncountered = false;
			  foreach ( string ruleSpec in ruleSpecs )
			  {
					ruleSpec = ruleSpec.Trim();

					IList<Filter<ServerInfo>> filterChain = new List<Filter<ServerInfo>>();
					string[] filterSpecs = ruleSpec.Split( "->", true );
					bool allFilterEncountered = false;
					foreach ( string filterSpec in filterSpecs )
					{
						 filterSpec = filterSpec.Trim();

						 string namePart;
						 string argsPart;
						 {
							  string[] nameAndArgs = filterSpec.Split( "\\(", true );

							  if ( nameAndArgs.Length != 2 )
							  {
									throw new InvalidFilterSpecification( format( "Syntax error filter specification: '%s'", filterSpec ) );
							  }

							  namePart = nameAndArgs[0].Trim();
							  argsPart = nameAndArgs[1].Trim();
						 }

						 if ( !argsPart.EndsWith( ")", StringComparison.Ordinal ) )
						 {
							  throw new InvalidFilterSpecification( format( "No closing parenthesis: '%s'", filterSpec ) );
						 }
						 argsPart = argsPart.Substring( 0, argsPart.Length - 1 );

						 string filterName = namePart.Trim();
						 if ( !filterName.matches( "\\w+" ) )
						 {
							  throw new InvalidFilterSpecification( format( "Syntax error filter name: '%s'", filterName ) );
						 }

						 string[] nonEmptyArgs = java.util.argsPart.Split( ",", true ).Select( string.Trim ).Where( s => !s.Empty ).ToList().ToArray(new string[0]);

						 foreach ( string arg in nonEmptyArgs )
						 {
							  if ( !arg.matches( "[\\w-]+" ) )
							  {
									throw new InvalidFilterSpecification( format( "Syntax error argument: '%s'", arg ) );
							  }
						 }

						 if ( haltFilterEncountered )
						 {
							  if ( filterChain.Count > 0 )
							  {
									throw new InvalidFilterSpecification( format( "Filter 'halt' may not be followed by other filters: '%s'", ruleSpec ) );
							  }
							  else
							  {
									throw new InvalidFilterSpecification( format( "Rule 'halt' may not followed by other rules: '%s'", filterConfig ) );
							  }
						 }

						 Filter<ServerInfo> filter = FilterFor( filterName, nonEmptyArgs );

						 if ( filter == HaltFilter.Instance )
						 {
							  if ( filterChain.Count != 0 )
							  {
									throw new InvalidFilterSpecification( format( "Filter 'halt' must be the only filter in a rule: '%s'", ruleSpec ) );
							  }
							  haltFilterEncountered = true;
						 }
						 else if ( filter == IdentityFilter.INSTANCE )
						 {
							  /* The all() filter is implicit and unnecessary, but still allowed in the beginning of a rule for clarity
							   * and for allowing the actual rule consisting of only all() to be specified. */

							  if ( allFilterEncountered || filterChain.Count != 0 )
							  {
									throw new InvalidFilterSpecification( format( "Filter 'all' is implicit but allowed only first in a rule: '%s'", ruleSpec ) );
							  }

							  allFilterEncountered = true;
						 }
						 else
						 {
							  filterChain.Add( filter );
						 }
					}

					if ( filterChain.Count > 0 )
					{
						 rules.Add( new FilterChain<>( filterChain ) );
					}
			  }

			  if ( !haltFilterEncountered )
			  {
					/* we implicitly add the all() rule to the end if there was no explicit halt() */
					rules.Add( new FilterChain<>( singletonList( IdentityFilter.@as() ) ) );
			  }

			  return new FirstValidRule<ServerInfo>( rules );
		 }
	}

}