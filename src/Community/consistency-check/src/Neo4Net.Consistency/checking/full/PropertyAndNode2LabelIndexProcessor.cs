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
namespace Neo4Net.Consistency.checking.full
{

	using Neo4Net.Consistency.checking;
	using Neo4Net.Consistency.checking;
	using CacheAccess = Neo4Net.Consistency.checking.cache.CacheAccess;
	using Neo4Net.Consistency.checking.full.MandatoryProperties;
	using IndexAccessors = Neo4Net.Consistency.checking.index.IndexAccessors;
	using ConsistencyReport = Neo4Net.Consistency.report.ConsistencyReport;
	using ConsistencyReport_NodeConsistencyReport = Neo4Net.Consistency.report.ConsistencyReport_NodeConsistencyReport;
	using ConsistencyReporter = Neo4Net.Consistency.report.ConsistencyReporter;
	using NodeRecord = Neo4Net.Kernel.Impl.Store.Records.NodeRecord;
	using PropertyRecord = Neo4Net.Kernel.Impl.Store.Records.PropertyRecord;

	/// <summary>
	/// Processor of node records with the context of how they're indexed.
	/// </summary>
	public class PropertyAndNode2LabelIndexProcessor : RecordProcessor_Adapter<NodeRecord>
	{
		 private readonly ConsistencyReporter _reporter;
		 private readonly RecordCheck<NodeRecord, ConsistencyReport_NodeConsistencyReport> _nodeIndexCheck;
		 private readonly RecordCheck<PropertyRecord, Neo4Net.Consistency.report.ConsistencyReport_PropertyConsistencyReport> _propertyCheck;
		 private readonly CacheAccess _cacheAccess;
		 private readonly System.Func<NodeRecord, Check<NodeRecord, ConsistencyReport_NodeConsistencyReport>> _mandatoryProperties;

		 public PropertyAndNode2LabelIndexProcessor( ConsistencyReporter reporter, IndexAccessors indexes, PropertyReader propertyReader, CacheAccess cacheAccess, System.Func<NodeRecord, MandatoryProperties.Check<NodeRecord, ConsistencyReport_NodeConsistencyReport>> mandatoryProperties )
		 {
			  this._reporter = reporter;
			  this._cacheAccess = cacheAccess;
			  this._mandatoryProperties = mandatoryProperties;
			  this._nodeIndexCheck = new PropertyAndNodeIndexedCheck( indexes, propertyReader, cacheAccess );
			  this._propertyCheck = new PropertyRecordCheck();
		 }

		 public override void Process( NodeRecord nodeRecord )
		 {
			  _reporter.forNode( nodeRecord, _nodeIndexCheck );
			  Neo4Net.Consistency.checking.cache.CacheAccess_Client client = _cacheAccess.client();
			  using ( MandatoryProperties.Check<NodeRecord, ConsistencyReport_NodeConsistencyReport> mandatoryCheck = _mandatoryProperties.apply( nodeRecord ) )
			  {
					IEnumerable<PropertyRecord> properties = client.PropertiesFromCache;

					// We do this null-check here because even if nodeIndexCheck should provide the properties for us,
					// or an empty list at least, it may fail in one way or another and exception be caught by
					// broad exception handler in reporter. The caught exception will produce an ERROR so it will not
					// go by unnoticed.
					if ( properties != null )
					{
						 foreach ( PropertyRecord property in properties )
						 {
							  _reporter.forProperty( property, _propertyCheck );
							  mandatoryCheck.Receive( ChainCheck.keys( property ) );
						 }
					}
			  }
		 }
	}

}