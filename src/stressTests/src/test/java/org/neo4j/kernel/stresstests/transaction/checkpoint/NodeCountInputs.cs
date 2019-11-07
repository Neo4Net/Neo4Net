﻿/*
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
namespace Neo4Net.Kernel.stresstests.transaction.checkpoint
{

	using Neo4Net.@unsafe.Impl.Batchimport;
	using InputIterable = Neo4Net.@unsafe.Impl.Batchimport.InputIterable;
	using NumberArrayFactory = Neo4Net.@unsafe.Impl.Batchimport.cache.NumberArrayFactory;
	using IdMapper = Neo4Net.@unsafe.Impl.Batchimport.cache.idmapping.IdMapper;
	using IdMappers = Neo4Net.@unsafe.Impl.Batchimport.cache.idmapping.IdMappers;
	using Collector = Neo4Net.@unsafe.Impl.Batchimport.input.Collector;
	using Collectors = Neo4Net.@unsafe.Impl.Batchimport.input.Collectors;
	using Group = Neo4Net.@unsafe.Impl.Batchimport.input.Group;
	using Input = Neo4Net.@unsafe.Impl.Batchimport.input.Input;
	using Value = Neo4Net.Values.Storable.Value;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.@unsafe.impl.batchimport.input.Inputs.knownEstimates;

	public class NodeCountInputs : Input
	{
		 private static readonly object[] _properties = new object[]{ "a", 10, "b", 10, "c", 10, "d", 10, "e", 10, "f", 10, "g", 10, "h", 10, "i", 10, "j", 10, "k", 10, "l", 10, "m", 10, "o", 10, "p", 10, "q", 10, "r", 10, "s", 10 };
		 private static readonly string[] _labels = new string[]{ "a", "b", "c", "d" };

		 private readonly long _nodeCount;
		 private readonly Collector _bad = Collectors.silentBadCollector( 0 );

		 public NodeCountInputs( long nodeCount )
		 {
			  this._nodeCount = nodeCount;
		 }

		 public override InputIterable Nodes()
		 {
			  return () => new GeneratingInputIterator<>(_nodeCount, 1_000, batch => null, (GeneratingInputIterator.Generator<Void>)(state, visitor, id) =>
			  {
			  visitor.id( id, Group.GLOBAL );
			  visitor.labels( _labels );
			  for ( int i = 0; i < _properties.Length; i++ )
			  {
				  visitor.property( ( string ) _properties[i++], _properties[i] );
			  }
			  }, 0);
		 }

		 public override InputIterable Relationships()
		 {
			  return GeneratingInputIterator.EMPTY_ITERABLE;
		 }

		 public override IdMapper IdMapper( NumberArrayFactory numberArrayFactory )
		 {
			  return IdMappers.actual();
		 }

		 public override Collector BadCollector()
		 {
			  return _bad;
		 }

		 public override Neo4Net.@unsafe.Impl.Batchimport.input.Input_Estimates CalculateEstimates( System.Func<Value[], int> valueSizeCalculator )
		 {
			  return knownEstimates( _nodeCount, 0, _nodeCount * _properties.Length / 2, 0, _nodeCount * _properties.Length / 2 * Long.BYTES, 0, _nodeCount * _labels.Length );
		 }
	}

}