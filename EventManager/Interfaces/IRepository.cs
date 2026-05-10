using EventManager.Models;

namespace EventManager.Interfaces;

/// <summary>
/// Универсальный репозиторий для управления данными сущностей, предоставляющий методы для получения, добавления, обновления и удаления объектов типа <typeparamref name="T"/>.
/// </summary>
public interface IRepository<T>
{
    /// <summary>
    /// Возвращает объект с указанным идентификатором.
    /// </summary>
    /// <param name="id">Идентификатор объекта.</param>
    /// <returns>Объект <typeparamref name="T"/>, если найден; иначе <see langword="null"/>.</returns>
    T? GetById(Guid id);

    /// <summary>
    /// Возвращает неизменяемую коллекцию всех объектов.
    /// </summary>
    /// <returns>Коллекция объектов <typeparamref name="T"/>. Если объекты отсутствуют, возвращается пустая коллекция.</returns>
    IReadOnlyCollection<T> GetAll();

    /// <summary>
    /// Добавляет новый объект в репозиторий.
    /// </summary>
    /// <param name="entityToAdd">Объект, который требуется добавить.</param>
    void Add(T entityToAdd);

    /// <summary>
    /// Обновляет существующий объект с указанным идентификатором.
    /// </summary>
    /// <param name="id">Идентификатор объекта.</param>
    /// <param name="entityToUpdate">Новые данные объекта.</param>
    void Update(Guid id, T entityToUpdate);

    /// <summary>
    /// Удаляет объект из репозитория.
    /// </summary>
    /// <param name="entityToDelete">Объект, который требуется удалить.</param>
    void Delete(T entityToDelete);
}
