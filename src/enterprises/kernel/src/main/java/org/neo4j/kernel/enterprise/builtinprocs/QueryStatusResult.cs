using System;
using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
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
namespace Neo4Net.Kernel.enterprise.builtinprocs
{

	using Node = Neo4Net.Graphdb.Node;
	using Relationship = Neo4Net.Graphdb.Relationship;
	using CRS = Neo4Net.Graphdb.spatial.CRS;
	using Coordinate = Neo4Net.Graphdb.spatial.Coordinate;
	using Point = Neo4Net.Graphdb.spatial.Point;
	using InvalidArgumentsException = Neo4Net.Kernel.Api.Exceptions.InvalidArgumentsException;
	using ExecutingQuery = Neo4Net.Kernel.api.query.ExecutingQuery;
	using QuerySnapshot = Neo4Net.Kernel.api.query.QuerySnapshot;
	using EmbeddedProxySPI = Neo4Net.Kernel.impl.core.EmbeddedProxySPI;
	using ClientConnectionInfo = Neo4Net.Kernel.impl.query.clientconnection.ClientConnectionInfo;
	using Neo4Net.Kernel.impl.util;
	using CoordinateReferenceSystem = Neo4Net.Values.Storable.CoordinateReferenceSystem;
	using MapValue = Neo4Net.Values.@virtual.MapValue;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.enterprise.builtinprocs.ProceduresTimeFormatHelper.formatInterval;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.enterprise.builtinprocs.ProceduresTimeFormatHelper.formatTime;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.enterprise.builtinprocs.QueryId.ofInternalId;

	public class QueryStatusResult
	{
		 public readonly string QueryId;
		 public readonly string Username;
		 public readonly IDictionary<string, object> MetaData;
		 public readonly string Query;
		 public readonly IDictionary<string, object> Parameters;
		 /// <summary>
		 /// @since Neo4j 3.2 </summary>
		 public readonly string Planner;
		 /// <summary>
		 /// @since Neo4j 3.2 </summary>
		 public readonly string Runtime;
		 /// <summary>
		 /// @since Neo4j 3.2 </summary>
		 public readonly IList<IDictionary<string, string>> Indexes;
		 public readonly string StartTime;
		 [Obsolete]
		 public readonly string ElapsedTime;
		 [Obsolete]
		 public readonly string ConnectionDetails;
		 /// <summary>
		 /// @since Neo4j 3.2 </summary>
		 public readonly string Protocol;
		 /// <summary>
		 /// @since Neo4j 3.2 </summary>
		 public readonly string ClientAddress;
		 /// <summary>
		 /// @since Neo4j 3.2 </summary>
		 public readonly string RequestUri;
		 /// <summary>
		 /// @since Neo4j 3.2 </summary>
		 public readonly string Status;
		 /// <summary>
		 /// @since Neo4j 3.2 </summary>
		 public readonly IDictionary<string, object> ResourceInformation;
		 /// <summary>
		 /// @since Neo4j 3.2 </summary>
		 public readonly long ActiveLockCount;
		 /// <summary>
		 /// @since Neo4j 3.2 </summary>
		 public readonly long ElapsedTimeMillis; // TODO: this field should be of a Duration type (when Cypher supports that)
		 /// <summary>
		 /// @since Neo4j 3.2, will be {@code null} if measuring CPU time is not supported. </summary>
		 public readonly long? CpuTimeMillis; // TODO: we want this field to be of a Duration type (when Cypher supports that)
		 /// <summary>
		 /// @since Neo4j 3.2 </summary>
		 public readonly long WaitTimeMillis; // TODO: we want this field to be of a Duration type (when Cypher supports that)
		 /// <summary>
		 /// @since Neo4j 3.2 </summary>
		 public readonly long? IdleTimeMillis; // TODO: we want this field to be of a Duration type (when Cypher supports that)
		 /// <summary>
		 /// @since Neo4j 3.2, will be {@code null} if measuring allocation is not supported. </summary>
		 public readonly long? AllocatedBytes;
		 /// <summary>
		 /// @since Neo4j 3.2 </summary>
		 public readonly long PageHits;
		 /// <summary>
		 /// @since Neo4j 3.2 </summary>
		 public readonly long PageFaults;
		 /// <summary>
		 /// @since Neo4j 3.5 </summary>
		 public readonly string ConnectionId;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: QueryStatusResult(org.neo4j.kernel.api.query.ExecutingQuery query, org.neo4j.kernel.impl.core.EmbeddedProxySPI manager, java.time.ZoneId zoneId) throws org.neo4j.kernel.api.exceptions.InvalidArgumentsException
		 internal QueryStatusResult( ExecutingQuery query, EmbeddedProxySPI manager, ZoneId zoneId ) : this( query.Snapshot(), manager, zoneId )
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private QueryStatusResult(org.neo4j.kernel.api.query.QuerySnapshot query, org.neo4j.kernel.impl.core.EmbeddedProxySPI manager, java.time.ZoneId zoneId) throws org.neo4j.kernel.api.exceptions.InvalidArgumentsException
		 private QueryStatusResult( QuerySnapshot query, EmbeddedProxySPI manager, ZoneId zoneId )
		 {
			  this.QueryId = ofInternalId( query.InternalQueryId() ).ToString();
			  this.Username = query.Username();
			  this.Query = query.QueryText();
			  this.Parameters = AsRawMap( query.QueryParameters(), new ParameterWriter(manager) );
			  this.StartTime = formatTime( query.StartTimestampMillis(), zoneId );
			  this.ElapsedTimeMillis = MicrosAsMillis( query.ElapsedTimeMicros() ).Value;
			  this.ElapsedTime = formatInterval( ElapsedTimeMillis );
			  ClientConnectionInfo clientConnection = query.ClientConnection();
			  this.ConnectionDetails = clientConnection.AsConnectionDetails();
			  this.Protocol = clientConnection.Protocol();
			  this.ClientAddress = clientConnection.ClientAddress();
			  this.RequestUri = clientConnection.RequestURI();
			  this.MetaData = query.TransactionAnnotationData();
			  this.CpuTimeMillis = MicrosAsMillis( query.CpuTimeMicros() );
			  this.Status = query.Status();
			  this.ResourceInformation = query.ResourceInformation();
			  this.ActiveLockCount = query.ActiveLockCount();
			  this.WaitTimeMillis = MicrosAsMillis( query.WaitTimeMicros() ).Value;
			  this.IdleTimeMillis = MicrosAsMillis( query.IdleTimeMicros() );
			  this.Planner = query.Planner();
			  this.Runtime = query.Runtime();
			  this.Indexes = query.Indexes();
			  this.AllocatedBytes = query.AllocatedBytes();
			  this.PageHits = query.PageHits();
			  this.PageFaults = query.PageFaults();
			  this.ConnectionId = clientConnection.ConnectionId();
		 }

		 private IDictionary<string, object> AsRawMap( MapValue mapValue, ParameterWriter writer )
		 {
			  Dictionary<string, object> map = new Dictionary<string, object>();
			  mapValue.Foreach((s, value) =>
			  {
				value.writeTo( writer );
				map[s] = writer.Value();
			  });
			  return map;
		 }

		 /// <summary>
		 /// Converts microseconds to milliseconds.
		 /// </summary>
		 /// <param name="micros">
		 /// @return </param>
		 private long? MicrosAsMillis( long? micros )
		 {
			  return micros == null ? null : TimeUnit.MICROSECONDS.toMillis( micros );
		 }

		 private class ParameterWriter : BaseToObjectValueWriter<Exception>
		 {
			  internal readonly EmbeddedProxySPI NodeManager;

			  internal ParameterWriter( EmbeddedProxySPI nodeManager )
			  {
					this.NodeManager = nodeManager;
			  }

			  protected internal override Node NewNodeProxyById( long id )
			  {
					return NodeManager.newNodeProxy( id );
			  }

			  protected internal override Relationship NewRelationshipProxyById( long id )
			  {
					return NodeManager.newRelationshipProxy( id );
			  }

			  protected internal override Point NewPoint( CoordinateReferenceSystem crs, double[] coordinate )
			  {
					return new PointAnonymousInnerClass( this, crs, coordinate );
			  }

			  private class PointAnonymousInnerClass : Point
			  {
				  private readonly ParameterWriter _outerInstance;

				  private CoordinateReferenceSystem _crs;
				  private double[] _coordinate;

				  public PointAnonymousInnerClass( ParameterWriter outerInstance, CoordinateReferenceSystem crs, double[] coordinate )
				  {
					  this.outerInstance = outerInstance;
					  this._crs = crs;
					  this._coordinate = coordinate;
				  }

				  public string GeometryType
				  {
					  get
					  {
							return "Point";
					  }
				  }

				  public IList<Coordinate> Coordinates
				  {
					  get
					  {
							return singletonList( new Coordinate( _coordinate ) );
					  }
				  }

				  public CRS CRS
				  {
					  get
					  {
							return _crs;
					  }
				  }
			  }
		 }
	}

}