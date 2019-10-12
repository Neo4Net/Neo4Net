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
namespace Org.Neo4j.Udc
{
	using Edition = Org.Neo4j.Kernel.impl.factory.Edition;
	using OperationalMode = Org.Neo4j.Kernel.impl.factory.OperationalMode;
	using DecayingFlags = Org.Neo4j.Util.concurrent.DecayingFlags;
	using Key = Org.Neo4j.Util.concurrent.DecayingFlags.Key;
	using Org.Neo4j.Util.concurrent;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.udc.UsageDataKey.key;

	/// <summary>
	/// Inventory of common keys. This list is not exhaustive, and all items listed may not be available.
	/// Still, this serves as a useful starting point for what you can expect to find, and new items added are
	/// encouraged to have their keys listed here.
	/// </summary>
	public class UsageDataKeys
	{
		 /// <summary>
		 /// Edition of Neo4j running, eg 'community' or 'enterprise' </summary>
		 public static readonly UsageDataKey<Edition> Edition = key( "neo4j.edition", Edition.unknown );

		 /// <summary>
		 /// Version of Neo4j running, eg. 1.2.3-RC1 </summary>
		 public static readonly UsageDataKey<string> Version = key( "neo4j.version", "N/A" );

		 /// <summary>
		 /// Revision of Neo4j running, a link back to source control revision ids. </summary>
		 public static readonly UsageDataKey<string> Revision = key( "neo4j.revision", "N/A" );

		 /// <summary>
		 /// Operational mode of the database </summary>
		 public static readonly UsageDataKey<OperationalMode> OperationalMode = key( "neo4j.opMode", OperationalMode.unknown );

		 /// <summary>
		 /// Self-reported names of clients connecting to us. </summary>
		 public static readonly UsageDataKey<RecentK<string>> ClientNames = key( "neo4j.clientNames", () => new RecentK<RecentK<string>>(10) );

		 /// <summary>
		 /// Cluster server ID </summary>
		 public static readonly UsageDataKey<string> ServerId = key( "neo4j.serverId" );

		 /// <summary>
		 /// Tracks features in use, including decay such that features that are not
		 /// used for a while are marked as no longer in use.
		 /// 
		 /// Decay is handled by an external mechanism invoking a 'sweep' method on this
		 /// DecayingFlags instance. See usages of this field to find where that happens.
		 /// </summary>
		 public static readonly UsageDataKey<DecayingFlags> Features = key( "neo4j.features", () => new DecayingFlags(7) );

		 public interface Features
		 {
			  // Note: The indexes used here is how we track which feature a flag
			  //       refers to. Be very careful about re-using indexes so features
			  //       don't get confused.

		 }

		 public static class Features_Fields
		 {
			 private readonly UsageDataKeys _outerInstance;

			 public Features_Fields( UsageDataKeys outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  public static readonly DecayingFlags.Key HttpCypherEndpoint = new DecayingFlags.Key( 0 );
			  public static readonly DecayingFlags.Key HttpTxEndpoint = new DecayingFlags.Key( 1 );
			  public static readonly DecayingFlags.Key HttpBatchEndpoint = new DecayingFlags.Key( 2 );
			  public static readonly DecayingFlags.Key Bolt = new DecayingFlags.Key( 3 );
		 }

		 private UsageDataKeys()
		 {
		 }
	}

}