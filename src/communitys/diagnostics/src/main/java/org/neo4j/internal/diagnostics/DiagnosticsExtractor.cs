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
namespace Neo4Net.@internal.Diagnostics
{
	using Logger = Neo4Net.Logging.Logger;

	/// <summary>
	/// An object that can extract diagnostics information from a source of a
	/// specific type.
	/// 
	/// @author Tobias Lindaaker <tobias.lindaaker@neotechnology.com>
	/// </summary>
	/// @param <T> the type of the source to extract diagnostics information from. </param>
	public interface DiagnosticsExtractor<T>
	{
		 /// <summary>
		 /// A <seealso cref="DiagnosticsExtractor"/> capable of
		 /// {@link DiagnosticsProvider#acceptDiagnosticsVisitor(Object) accepting
		 /// visitors}.
		 /// 
		 /// @author Tobias Lindaaker <tobias.lindaaker@neotechnology.com>
		 /// </summary>
		 /// @param <T> the type of the source to extract diagnostics information
		 ///            from. </param>

		 /// <summary>
		 /// Dump the diagnostic information of the specified source for the specified
		 /// <seealso cref="DiagnosticsPhase phase"/> to the provided <seealso cref="Logger logger"/>.
		 /// </summary>
		 /// <seealso cref= DiagnosticsProvider#dump(DiagnosticsPhase, Logger) </seealso>
		 /// <param name="source"> the source to get diagnostics information from. </param>
		 /// <param name="phase"> the <seealso cref="DiagnosticsPhase phase"/> to dump information for. </param>
		 /// <param name="logger"> the <seealso cref="Logger logger"/> to dump information to. </param>
		 void DumpDiagnostics( T source, DiagnosticsPhase phase, Logger logger );
	}

	 public interface DiagnosticsExtractor_VisitableDiagnostics<T> : DiagnosticsExtractor<T>
	 {
		  /// <summary>
		  /// Accept a visitor that may or may not be capable of visiting this
		  /// object.
		  /// </summary>
		  /// <seealso cref= DiagnosticsProvider#acceptDiagnosticsVisitor(Object) </seealso>
		  /// <param name="source"> the source to get diagnostics information from. </param>
		  /// <param name="visitor"> the visitor visiting the diagnostics information. </param>
		  void DispatchDiagnosticsVisitor( T source, object visitor );
	 }

}