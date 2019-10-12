using System;
using System.Collections.Generic;
using System.Reflection;

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
namespace Neo4Net.Kernel.configuration.ssl
{

	using InvalidSettingException = Neo4Net.Graphdb.config.InvalidSettingException;
	using Neo4Net.Graphdb.config;
	using Neo4Net.Graphdb.config;


	public class SslPolicyConfigValidator : SettingGroup<object>
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.util.Map<String,String> validate(java.util.Map<String,String> params, System.Action<String> warningConsumer) throws org.neo4j.graphdb.config.InvalidSettingException
		 public override IDictionary<string, string> Validate( IDictionary<string, string> @params, System.Action<string> warningConsumer )
		 {
			  IDictionary<string, string> validatedParams = new Dictionary<string, string>();

			  ISet<string> validShortKeys = ExtractValidShortKeys();
			  string groupSettingPrefix = GroupPrefix();

			  Pattern groupSettingPattern = Pattern.compile( Pattern.quote( groupSettingPrefix ) + "\\.([^.]+)\\.?(.+)?" );

			  ISet<string> policyNames = new HashSet<string>();

			  foreach ( KeyValuePair<string, string> paramsEntry in @params.SetOfKeyValuePairs() )
			  {
					string settingName = paramsEntry.Key;
					Matcher matcher = groupSettingPattern.matcher( settingName );
					if ( !matcher.matches() )
					{
						 continue;
					}

					policyNames.Add( matcher.group( 1 ) );
					string shortKey = matcher.group( 2 );

					if ( !validShortKeys.Contains( shortKey ) )
					{
						 throw new InvalidSettingException( "Invalid setting name: " + settingName );
					}

					validatedParams[settingName] = paramsEntry.Value;
			  }

			  foreach ( string policyName in policyNames )
			  {
					SslPolicyConfig policy = new SslPolicyConfig( policyName );

					if ( !@params.ContainsKey( policy.BaseDirectory.name() ) )
					{
						 throw new InvalidSettingException( "Missing mandatory setting: " + policy.BaseDirectory.name() );
					}
			  }

			  return validatedParams;
		 }

		 private string GroupPrefix()
		 {
			  return typeof( SslPolicyConfig ).getDeclaredAnnotation( typeof( Group ) ).value();
		 }

		 private ISet<string> ExtractValidShortKeys()
		 {
			  ISet<string> validSettingNames = new HashSet<string>();

			  string policyName = "example";
			  int prefixLength = GroupPrefix().Length + 1 + policyName.Length + 1; // dbms.ssl.policy.example.

			  SslPolicyConfig examplePolicy = new SslPolicyConfig( policyName );
			  System.Reflection.FieldInfo[] fields = examplePolicy.GetType().GetFields(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
			  foreach ( System.Reflection.FieldInfo field in fields )
			  {
					if ( Modifier.isStatic( field.Modifiers ) )
					{
						 continue;
					}

					try
					{
						 object obj = field.get( examplePolicy );
						 if ( obj is Setting )
						 {
							  string longKey = ( ( Setting ) obj ).name();
							  string shortKey = longKey.Substring( prefixLength );
							  validSettingNames.Add( shortKey );
						 }
					}
					catch ( IllegalAccessException e )
					{
						 throw new Exception( e );
					}
			  }
			  return validSettingNames;
		 }

		 public override IDictionary<string, object> Values( IDictionary<string, string> validConfig )
		 {
			  return emptyMap();
		 }

		 public override IList<Setting<object>> Settings( IDictionary<string, string> @params )
		 {
			  return emptyList();
		 }

		 public override bool Deprecated()
		 {
			  return false;
		 }

		 public override Optional<string> Replacement()
		 {
			  return empty();
		 }

		 public override bool Internal()
		 {
			  return false;
		 }

		 public override bool Secret()
		 {
			  return false;
		 }

		 public override Optional<string> DocumentedDefaultValue()
		 {
			  return empty();
		 }

		 public override string ValueDescription()
		 {
			  return "SSL policy configuration";
		 }

		 public override Optional<string> Description()
		 {
			  return empty();
		 }

		 public override bool Dynamic()
		 {
			  return false;
		 }
	}

}