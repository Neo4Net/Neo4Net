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
namespace Org.Neo4j.Server.rest.web
{

	/// <summary>
	/// Helps generate testable data models, using a RestfulGraphDatabase.
	/// 
	/// </summary>
	public class ModelBuilder
	{
		 private ModelBuilder()
		 {
		 }

		 public static DomainModel GenerateMatrix( RestfulGraphDatabase rgd )
		 {
			  string key = "key_get";
			  string value = "value";

			  DomainModel dm = new DomainModel();

			  DomainEntity thomas = new DomainEntity();
			  thomas.Properties["name"] = "Thomas Anderson";
			  thomas.Location = ( URI ) rgd.CreateNode( "{\"name\":\"" + "Thomas Anderson" + "\"}" ).Metadata.getFirst( "Location" );
			  dm.Add( thomas );

			  DomainEntity agent = new DomainEntity();
			  agent.Properties["name"] = "Agent Smith";
			  agent.Location = ( URI ) rgd.CreateNode( "{\"name\":\"" + "Agent Smith" + "\"}" ).Metadata.getFirst( "Location" );
			  dm.Add( agent );

			  dm.NodeIndexName = "matrixal-nodes";
			  dm.IndexedNodeKeyValues[key] = value;

			  dm.IndexedNodeUriToEntityMap[( URI ) rgd.AddToNodeIndex( dm.NodeIndexName, null, null, "{\"key\": \"" + key + "\", \"value\":\"" + value + "\", \"uri\": \"" + thomas.Location + "\"}" ).Metadata.getFirst( "Location" )] = thomas;
			  dm.IndexedNodeUriToEntityMap[( URI ) rgd.AddToNodeIndex( dm.NodeIndexName, null, null, "{\"key\": \"" + key + "\", \"value\":\"" + value + "\", \"uri\": \"" + agent.Location + "\"}" ).Metadata.getFirst( "Location" )] = agent;

			  return dm;
		 }

		 public class DomainEntity
		 {
			  public URI Location;
			  public IDictionary<string, string> Properties = new Dictionary<string, string>();
		 }

		 public class DomainModel
		 {
			  public IDictionary<URI, DomainEntity> NodeUriToEntityMap = new Dictionary<URI, DomainEntity>();
			  internal string NodeIndexName = "nodes";
			  public IDictionary<string, string> IndexedNodeKeyValues = new Dictionary<string, string>();
			  public IDictionary<URI, DomainEntity> IndexedNodeUriToEntityMap = new Dictionary<URI, DomainEntity>();

			  public virtual void Add( DomainEntity de )
			  {
					NodeUriToEntityMap[de.Location] = de;
			  }
		 }
	}

}